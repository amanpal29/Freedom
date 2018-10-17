// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Infrastructure.DependencyResolution;
    using System.Data.Entity.Migrations.Edm;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Migrations.Sql;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Xml.Linq;

    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal class EdmModelDiffer
    {
        private static readonly PrimitiveTypeKind[] _validIdentityTypes =
            {
                PrimitiveTypeKind.Byte,
                PrimitiveTypeKind.Decimal,
                PrimitiveTypeKind.Guid,
                PrimitiveTypeKind.Int16,
                PrimitiveTypeKind.Int32,
                PrimitiveTypeKind.Int64
            };

        /// <summary>
        ///     Exposed internally for testing.
        /// </summary>
        public class ModelMetadata
        {
            public XDocument Model { get; set; }
            public EdmItemCollection EdmItemCollection { get; set; }
            public StoreItemCollection StoreItemCollection { get; set; }
            public StorageEntityContainerMapping StorageEntityContainerMapping { get; set; }
            public DbProviderManifest ProviderManifest { get; set; }
            public DbProviderInfo ProviderInfo { get; set; }
        }

        private ModelMetadata _source;
        private ModelMetadata _target;

        private bool _consistentProviders;

        public IEnumerable<MigrationOperation> Diff(
            XDocument sourceModel,
            XDocument targetModel,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator = null,
            MigrationSqlGenerator migrationSqlGenerator = null)
        {
            DebugCheck.NotNull(sourceModel);
            DebugCheck.NotNull(targetModel);

            if (sourceModel == targetModel
                || XNode.DeepEquals(sourceModel, targetModel))
            {
                // Trivial checks before we do the hard stuff...
                
                return Enumerable.Empty<MigrationOperation>();
            }

            DbProviderInfo providerInfo;

            var storageMappingItemCollection
                = sourceModel.GetStorageMappingItemCollection(out providerInfo);

            var source
                = new ModelMetadata
                    {
                        Model = sourceModel,
                        EdmItemCollection = storageMappingItemCollection.EdmItemCollection,
                        StoreItemCollection = storageMappingItemCollection.StoreItemCollection,
                        StorageEntityContainerMapping
                            = storageMappingItemCollection.GetItems<StorageEntityContainerMapping>().Single(),
                        ProviderManifest = GetProviderManifest(providerInfo),
                        ProviderInfo = providerInfo
                    };

            storageMappingItemCollection
                = targetModel.GetStorageMappingItemCollection(out providerInfo);

            var target
                = new ModelMetadata
                    {
                        Model = targetModel,
                        EdmItemCollection = storageMappingItemCollection.EdmItemCollection,
                        StoreItemCollection = storageMappingItemCollection.StoreItemCollection,
                        StorageEntityContainerMapping
                            = storageMappingItemCollection.GetItems<StorageEntityContainerMapping>().Single(),
                        ProviderManifest = GetProviderManifest(providerInfo),
                        ProviderInfo = providerInfo
                    };

            return Diff(source, target, modificationCommandTreeGenerator, migrationSqlGenerator);
        }

        /// <summary>
        ///     For testing.
        /// </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public IEnumerable<MigrationOperation> Diff(
            ModelMetadata source,
            ModelMetadata target,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(source);
            DebugCheck.NotNull(target);

            _source = source;
            _target = target;

            _consistentProviders
                = _source.ProviderInfo.ProviderInvariantName.EqualsIgnoreCase(
                    _target.ProviderInfo.ProviderInvariantName)
                  && _source.ProviderInfo.ProviderManifestToken.EqualsIgnoreCase(
                      _target.ProviderInfo.ProviderManifestToken);

            var renamedColumns = FindRenamedColumns().ToList();
            var addedColumns = FindAddedColumns(renamedColumns).ToList();
            var alteredColumns = FindChangedColumns(renamedColumns).ToList();
            var removedColumns = FindRemovedColumns(renamedColumns).ToList();
            var orphanedColumns = FindOrphanedColumns(renamedColumns).ToList();
 
            var tablePairs = FindTablePairs().ToList();

            var renamedTables = FindRenamedTables(tablePairs).ToList();
            var movedTables = FindMovedTables(tablePairs).ToList();
            var addedTables = FindAddedTables(renamedTables, tablePairs).ToList();

            var columnNormalizedSourceModel = BuildColumnNormalizedSourceModel(renamedColumns);

            var assocationPairs = FindAssociationPairs(columnNormalizedSourceModel).ToList();

            var addedForeignKeys = FindAddedForeignKeys(assocationPairs).ToList();
            var removedTables = FindRemovedTables(renamedTables, tablePairs).ToList();
            var removedForeignKeys = FindRemovedForeignKeys(columnNormalizedSourceModel, assocationPairs).ToList();
            var changedPrimaryKeys = FindChangedPrimaryKeys(columnNormalizedSourceModel).ToList();

            var addedModificationFunctions
                = FindAddedModificationFunctions(modificationCommandTreeGenerator, migrationSqlGenerator)
                    .ToList();

            var changedModificationFunctions
                = FindChangedModificationFunctions(modificationCommandTreeGenerator, migrationSqlGenerator)
                    .ToList();

            var removedModificationFunctions = FindRemovedModificationFunctions().ToList();
            var renamedModificationFunctions = FindRenamedModificationFunctions().ToList();
            var movedModificationFunctions = FindMovedModificationFunctions().ToList();

            return HandleTransitiveRenameDependencies(renamedTables)
                .Concat<MigrationOperation>(movedTables)
                .Concat(removedForeignKeys)
                .Concat(removedForeignKeys.Select(fko => fko.CreateDropIndexOperation()))
                .Concat(orphanedColumns)
                .Concat(HandleTransitiveRenameDependencies(renamedColumns))
                .Concat(addedTables)
                .Concat(addedColumns)
                .Concat(alteredColumns)
                .Concat(changedPrimaryKeys)
                .Concat(addedForeignKeys.Select(fko => fko.CreateCreateIndexOperation()))
                .Concat(addedForeignKeys)
                .Concat(removedColumns)
                .Concat(removedTables)
                .Concat(addedModificationFunctions)
                .Concat(movedModificationFunctions)
                .Concat(renamedModificationFunctions)
                .Concat(changedModificationFunctions)
                .Concat(removedModificationFunctions)
                .ToList();
        }

        private static IEnumerable<RenameTableOperation> HandleTransitiveRenameDependencies(
            IList<RenameTableOperation> renameTableOperations)
        {
            DebugCheck.NotNull(renameTableOperations);

            return HandleTransitiveRenameDependencies(
                renameTableOperations,
                (rt1, rt2) =>
                    {
                        var databaseName1 = DatabaseName.Parse(rt1.Name);
                        var databaseName2 = DatabaseName.Parse(rt2.Name);

                        return databaseName1.Name.EqualsIgnoreCase(rt2.NewName)
                               && databaseName1.Schema.EqualsIgnoreCase(databaseName2.Schema);
                    },
                (t, rt) => new RenameTableOperation(t, rt.NewName),
                (rt, t) => rt.NewName = t);
        }

        private static IEnumerable<RenameColumnOperation> HandleTransitiveRenameDependencies(
            IList<RenameColumnOperation> renameColumnOperations)
        {
            DebugCheck.NotNull(renameColumnOperations);

            return HandleTransitiveRenameDependencies(
                renameColumnOperations,
                (rc1, rc2) => rc1.Table.EqualsIgnoreCase(rc2.Table)
                              && rc1.Name.EqualsIgnoreCase(rc2.NewName),
                (t, rc) => new RenameColumnOperation(rc.Table, t, rc.NewName),
                (rc, t) => rc.NewName = t);
        }

        private static IEnumerable<T> HandleTransitiveRenameDependencies<T>(
            IList<T> renameOperations,
            Func<T, T, bool> dependencyFinder,
            Func<string, T, T> renameCreator,
            Action<T, string> setNewName)
            where T : class
        {
            DebugCheck.NotNull(renameOperations);
            DebugCheck.NotNull(dependencyFinder);
            DebugCheck.NotNull(renameCreator);
            DebugCheck.NotNull(setNewName);

            var tempCounter = 0;
            var tempRenames = new List<T>();

            for (var i = 0; i < renameOperations.Count; i++)
            {
                var renameOperation = renameOperations[i];

                var dependentRename
                    = renameOperations
                        .Skip(i + 1)
                        .SingleOrDefault(rt => dependencyFinder(renameOperation, rt));

                if (dependentRename != null)
                {
                    var tempNewName = "__mig_tmp__" + tempCounter++;

                    tempRenames.Add(renameCreator(tempNewName, renameOperation));

                    setNewName(renameOperation, tempNewName);
                }

                yield return renameOperation;
            }

            foreach (var renameOperation in tempRenames)
            {
                yield return renameOperation;
            }
        }

        private XDocument BuildColumnNormalizedSourceModel(ICollection<RenameColumnOperation> renamedColumns)
        {
            DebugCheck.NotNull(renamedColumns);

            var columnNormalizedSourceModel = new XDocument(_source.Model); // clone

            var normalizedElements = new List<XElement>();

            renamedColumns.Each(
                rc =>
                    {
                        var entitySet
                            = (from es in _target.Model.Descendants(EdmXNames.Ssdl.EntitySetNames)
                               where
                                   GetSchemaQualifiedName(es.TableAttribute(), es.SchemaAttribute()).EqualsIgnoreCase(
                                       rc.Table)
                               select es.NameAttribute()).Single();

                        var principalDependents
                            = from pd in columnNormalizedSourceModel.Descendants(EdmXNames.Ssdl.PrincipalNames)
                                  .Concat(
                                      columnNormalizedSourceModel.Descendants(
                                          EdmXNames.Ssdl.DependentNames))
                              where pd.RoleAttribute().EqualsIgnoreCase(entitySet)
                              from pr in pd.Descendants(EdmXNames.Ssdl.PropertyRefNames)
                              where pr.NameAttribute().EqualsIgnoreCase(rc.Name)
                                    && !normalizedElements.Contains(pr)
                              select pr;

                        principalDependents.Each(
                            pr =>
                                {
                                    pr.SetAttributeValue("Name", rc.NewName);
                                    normalizedElements.Add(pr);
                                });

                        var keyProperties
                            = from et in columnNormalizedSourceModel.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                              where et.NameAttribute().EqualsIgnoreCase(entitySet)
                              from pr in
                                  et.Descendants(EdmXNames.Ssdl.KeyNames).Descendants(EdmXNames.Ssdl.PropertyRefNames)
                              where pr.NameAttribute().EqualsIgnoreCase(rc.Name)
                                    && !normalizedElements.Contains(pr)
                              select pr;

                        keyProperties.Each(
                            pr =>
                                {
                                    pr.SetAttributeValue("Name", rc.NewName);
                                    normalizedElements.Add(pr);
                                });
                    });

            return columnNormalizedSourceModel;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<MoveProcedureOperation> FindMovedModificationFunctions()
        {
            return
                (from esm1 in _source.StorageEntityContainerMapping.EntitySetMappings
                 from mfm1 in esm1.ModificationFunctionMappings
                 from esm2 in _target.StorageEntityContainerMapping.EntitySetMappings
                 from mfm2 in esm2.ModificationFunctionMappings
                 where mfm1.EntityType.Identity == mfm2.EntityType.Identity
                 from o in DiffModificationFunctionSchemas(mfm1, mfm2)
                 select o)
                    .Concat(
                        from asm1 in _source.StorageEntityContainerMapping.AssociationSetMappings
                        where asm1.ModificationFunctionMapping != null
                        from asm2 in _target.StorageEntityContainerMapping.AssociationSetMappings
                        where asm2.ModificationFunctionMapping != null
                              && asm1.ModificationFunctionMapping.AssociationSet.Identity
                              == asm2.ModificationFunctionMapping.AssociationSet.Identity
                        from o in DiffModificationFunctionSchemas(asm1.ModificationFunctionMapping, asm2.ModificationFunctionMapping)
                        select o);
        }

        private static IEnumerable<MoveProcedureOperation> DiffModificationFunctionSchemas(
            StorageEntityTypeModificationFunctionMapping sourceModificationFunctionMapping,
            StorageEntityTypeModificationFunctionMapping targetModificationFunctionMapping)
        {
            DebugCheck.NotNull(sourceModificationFunctionMapping);
            DebugCheck.NotNull(targetModificationFunctionMapping);

            if (!sourceModificationFunctionMapping.InsertFunctionMapping.Function.Schema
                     .EqualsOrdinal(targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema))
            {
                yield return new MoveProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName,
                        sourceModificationFunctionMapping.InsertFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema);
            }

            if (!sourceModificationFunctionMapping.UpdateFunctionMapping.Function.Schema
                     .EqualsOrdinal(targetModificationFunctionMapping.UpdateFunctionMapping.Function.Schema))
            {
                yield return new MoveProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName,
                        sourceModificationFunctionMapping.UpdateFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.UpdateFunctionMapping.Function.Schema);
            }

            if (!sourceModificationFunctionMapping.DeleteFunctionMapping.Function.Schema
                     .EqualsOrdinal(targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema))
            {
                yield return new MoveProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName,
                        sourceModificationFunctionMapping.DeleteFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema);
            }
        }

        private static IEnumerable<MoveProcedureOperation> DiffModificationFunctionSchemas(
            StorageAssociationSetModificationFunctionMapping sourceModificationFunctionMapping,
            StorageAssociationSetModificationFunctionMapping targetModificationFunctionMapping)
        {
            DebugCheck.NotNull(sourceModificationFunctionMapping);
            DebugCheck.NotNull(targetModificationFunctionMapping);

            if (!sourceModificationFunctionMapping.InsertFunctionMapping.Function.Schema
                     .EqualsOrdinal(targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema))
            {
                yield return new MoveProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName,
                        sourceModificationFunctionMapping.InsertFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema);
            }

            if (!sourceModificationFunctionMapping.DeleteFunctionMapping.Function.Schema
                     .EqualsOrdinal(targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema))
            {
                yield return new MoveProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName,
                        sourceModificationFunctionMapping.DeleteFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<CreateProcedureOperation> FindAddedModificationFunctions(
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
        {
            return
                (from esm1 in _target.StorageEntityContainerMapping.EntitySetMappings
                 from mfm1 in esm1.ModificationFunctionMappings
                 where !(from esm2 in _source.StorageEntityContainerMapping.EntitySetMappings
                         from mfm2 in esm2.ModificationFunctionMappings
                         where mfm1.EntityType.Identity == mfm2.EntityType.Identity
                         select mfm2
                        ).Any()
                 from o in BuildCreateProcedureOperations(mfm1, modificationCommandTreeGenerator, migrationSqlGenerator)
                 select o)
                    .Concat(
                        from asm1 in _target.StorageEntityContainerMapping.AssociationSetMappings
                        where asm1.ModificationFunctionMapping != null
                        where !(from asm2 in _source.StorageEntityContainerMapping.AssociationSetMappings
                                where asm2.ModificationFunctionMapping != null
                                      && asm1.ModificationFunctionMapping.AssociationSet.Identity
                                      == asm2.ModificationFunctionMapping.AssociationSet.Identity
                                select asm2.ModificationFunctionMapping
                               ).Any()
                        from o in BuildCreateProcedureOperations(
                            asm1.ModificationFunctionMapping,
                            modificationCommandTreeGenerator,
                            migrationSqlGenerator)
                        select o);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<RenameProcedureOperation> FindRenamedModificationFunctions()
        {
            return
                (from esm1 in _source.StorageEntityContainerMapping.EntitySetMappings
                 from mfm1 in esm1.ModificationFunctionMappings
                 from esm2 in _target.StorageEntityContainerMapping.EntitySetMappings
                 from mfm2 in esm2.ModificationFunctionMappings
                 where mfm1.EntityType.Identity == mfm2.EntityType.Identity
                 from o in DiffModificationFunctionNames(mfm1, mfm2)
                 select o)
                    .Concat(
                        from asm1 in _source.StorageEntityContainerMapping.AssociationSetMappings
                        where asm1.ModificationFunctionMapping != null
                        from asm2 in _target.StorageEntityContainerMapping.AssociationSetMappings
                        where asm2.ModificationFunctionMapping != null
                              && asm1.ModificationFunctionMapping.AssociationSet.Identity
                              == asm2.ModificationFunctionMapping.AssociationSet.Identity
                        from o in DiffModificationFunctionNames(asm1.ModificationFunctionMapping, asm2.ModificationFunctionMapping)
                        select o);
        }

        private static IEnumerable<RenameProcedureOperation> DiffModificationFunctionNames(
            StorageAssociationSetModificationFunctionMapping sourceModificationFunctionMapping,
            StorageAssociationSetModificationFunctionMapping targetModificationFunctionMapping)
        {
            DebugCheck.NotNull(sourceModificationFunctionMapping);
            DebugCheck.NotNull(targetModificationFunctionMapping);

            if (!sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName
                     .EqualsOrdinal(targetModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName))
            {
                yield return new RenameProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName,
                        targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName);
            }

            if (!sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName
                     .EqualsOrdinal(targetModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName))
            {
                yield return new RenameProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName,
                        targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName);
            }
        }

        private static IEnumerable<RenameProcedureOperation> DiffModificationFunctionNames(
            StorageEntityTypeModificationFunctionMapping sourceModificationFunctionMapping,
            StorageEntityTypeModificationFunctionMapping targetModificationFunctionMapping)
        {
            DebugCheck.NotNull(sourceModificationFunctionMapping);
            DebugCheck.NotNull(targetModificationFunctionMapping);

            if (!sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName
                     .EqualsOrdinal(targetModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName))
            {
                yield return new RenameProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName,
                        targetModificationFunctionMapping.InsertFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName);
            }

            if (!sourceModificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName
                     .EqualsOrdinal(targetModificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName))
            {
                yield return new RenameProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName,
                        targetModificationFunctionMapping.UpdateFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName);
            }

            if (!sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName
                     .EqualsOrdinal(targetModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName))
            {
                yield return new RenameProcedureOperation(
                    GetSchemaQualifiedName(
                        sourceModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName,
                        targetModificationFunctionMapping.DeleteFunctionMapping.Function.Schema),
                    targetModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<AlterProcedureOperation> FindChangedModificationFunctions(
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator, MigrationSqlGenerator migrationSqlGenerator)
        {
            return
                (from esm1 in _source.StorageEntityContainerMapping.EntitySetMappings
                 from mfm1 in esm1.ModificationFunctionMappings
                 from esm2 in _target.StorageEntityContainerMapping.EntitySetMappings
                 from mfm2 in esm2.ModificationFunctionMappings
                 where mfm1.EntityType.Identity == mfm2.EntityType.Identity
                 from o in DiffModificationFunctions(mfm1, mfm2, modificationCommandTreeGenerator, migrationSqlGenerator)
                 select o)
                    .Concat(
                        from asm1 in _source.StorageEntityContainerMapping.AssociationSetMappings
                        where asm1.ModificationFunctionMapping != null
                        from asm2 in _target.StorageEntityContainerMapping.AssociationSetMappings
                        where asm2.ModificationFunctionMapping != null
                              && asm1.ModificationFunctionMapping.AssociationSet.Identity
                              == asm2.ModificationFunctionMapping.AssociationSet.Identity
                        from o in DiffModificationFunctions(
                            asm1.ModificationFunctionMapping,
                            asm2.ModificationFunctionMapping,
                            modificationCommandTreeGenerator,
                            migrationSqlGenerator)
                        select o);
        }

        private IEnumerable<AlterProcedureOperation> DiffModificationFunctions(
            StorageAssociationSetModificationFunctionMapping sourceModificationFunctionMapping,
            StorageAssociationSetModificationFunctionMapping targetModificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(sourceModificationFunctionMapping);
            DebugCheck.NotNull(targetModificationFunctionMapping);

            if (!DiffModificationFunction(
                sourceModificationFunctionMapping.InsertFunctionMapping,
                targetModificationFunctionMapping.InsertFunctionMapping))
            {
                yield return BuildAlterProcedureOperation(
                    targetModificationFunctionMapping.InsertFunctionMapping.Function,
                    GenerateInsertFunctionBody(
                        targetModificationFunctionMapping,
                        modificationCommandTreeGenerator,
                        migrationSqlGenerator));
            }

            if (!DiffModificationFunction(
                sourceModificationFunctionMapping.DeleteFunctionMapping,
                targetModificationFunctionMapping.DeleteFunctionMapping))
            {
                yield return BuildAlterProcedureOperation(
                    targetModificationFunctionMapping.DeleteFunctionMapping.Function,
                    GenerateDeleteFunctionBody(
                        targetModificationFunctionMapping,
                        modificationCommandTreeGenerator,
                        migrationSqlGenerator));
            }
        }

        private IEnumerable<AlterProcedureOperation> DiffModificationFunctions(
            StorageEntityTypeModificationFunctionMapping sourceModificationFunctionMapping,
            StorageEntityTypeModificationFunctionMapping targetModificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(sourceModificationFunctionMapping);
            DebugCheck.NotNull(targetModificationFunctionMapping);

            if (!DiffModificationFunction(
                sourceModificationFunctionMapping.InsertFunctionMapping,
                targetModificationFunctionMapping.InsertFunctionMapping))
            {
                yield return BuildAlterProcedureOperation(
                    targetModificationFunctionMapping.InsertFunctionMapping.Function,
                    GenerateInsertFunctionBody(
                        targetModificationFunctionMapping,
                        modificationCommandTreeGenerator,
                        migrationSqlGenerator));
            }

            if (!DiffModificationFunction(
                sourceModificationFunctionMapping.UpdateFunctionMapping,
                targetModificationFunctionMapping.UpdateFunctionMapping))
            {
                yield return BuildAlterProcedureOperation(
                    targetModificationFunctionMapping.UpdateFunctionMapping.Function,
                    GenerateUpdateFunctionBody(
                        targetModificationFunctionMapping,
                        modificationCommandTreeGenerator,
                        migrationSqlGenerator));
            }

            if (!DiffModificationFunction(
                sourceModificationFunctionMapping.DeleteFunctionMapping,
                targetModificationFunctionMapping.DeleteFunctionMapping))
            {
                yield return BuildAlterProcedureOperation(
                    targetModificationFunctionMapping.DeleteFunctionMapping.Function,
                    GenerateDeleteFunctionBody(
                        targetModificationFunctionMapping,
                        modificationCommandTreeGenerator,
                        migrationSqlGenerator));
            }
        }

        private string GenerateInsertFunctionBody(
            StorageEntityTypeModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            return GenerateFunctionBody(
                modificationFunctionMapping,
                (m, s) => m.GenerateInsert(s),
                modificationCommandTreeGenerator,
                migrationSqlGenerator,
                modificationFunctionMapping.InsertFunctionMapping.Function.FunctionName,
                rowsAffectedParameterName: null);
        }

        private string GenerateInsertFunctionBody(
            StorageAssociationSetModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            return GenerateFunctionBody(
                modificationFunctionMapping,
                (m, s) => m.GenerateAssociationInsert(s),
                modificationCommandTreeGenerator,
                migrationSqlGenerator,
                rowsAffectedParameterName: null);
        }

        private string GenerateUpdateFunctionBody(
            StorageEntityTypeModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            return GenerateFunctionBody(
                modificationFunctionMapping,
                (m, s) => m.GenerateUpdate(s),
                modificationCommandTreeGenerator,
                migrationSqlGenerator,
                modificationFunctionMapping.UpdateFunctionMapping.Function.FunctionName,
                rowsAffectedParameterName: modificationFunctionMapping.UpdateFunctionMapping.RowsAffectedParameterName);
        }

        private string GenerateDeleteFunctionBody(
            StorageEntityTypeModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            return GenerateFunctionBody(
                modificationFunctionMapping,
                (m, s) => m.GenerateDelete(s),
                modificationCommandTreeGenerator,
                migrationSqlGenerator,
                modificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName,
                rowsAffectedParameterName: modificationFunctionMapping.DeleteFunctionMapping.RowsAffectedParameterName);
        }

        private string GenerateDeleteFunctionBody(
            StorageAssociationSetModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            return GenerateFunctionBody(
                modificationFunctionMapping,
                (m, s) => m.GenerateAssociationDelete(s),
                modificationCommandTreeGenerator,
                migrationSqlGenerator,
                rowsAffectedParameterName: modificationFunctionMapping.DeleteFunctionMapping.RowsAffectedParameterName);
        }

        private string GenerateFunctionBody<TCommandTree>(
            StorageEntityTypeModificationFunctionMapping modificationFunctionMapping,
            Func<ModificationCommandTreeGenerator, string, IEnumerable<TCommandTree>> treeGenerator,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator,
            string functionName,
            string rowsAffectedParameterName)
            where TCommandTree : DbModificationCommandTree
        {
            DebugCheck.NotNull(modificationFunctionMapping);
            DebugCheck.NotNull(treeGenerator);

            var commandTrees = new TCommandTree[0];

            if (modificationCommandTreeGenerator != null)
            {
                var dynamicToFunctionModificationCommandConverter
                    = new DynamicToFunctionModificationCommandConverter(
                        modificationFunctionMapping,
                        _target.StorageEntityContainerMapping);

                try
                {
                    commandTrees
                        = dynamicToFunctionModificationCommandConverter
                            .Convert(treeGenerator(modificationCommandTreeGenerator.Value, modificationFunctionMapping.EntityType.Identity))
                            .ToArray();
                }
                catch (UpdateException e)
                {
                    throw new InvalidOperationException(
                        Strings.ErrorGeneratingCommandTree(
                            functionName,
                            modificationFunctionMapping.EntityType.Name), e);
                }
            }

            return GenerateFunctionBody(migrationSqlGenerator, rowsAffectedParameterName, commandTrees);
        }

        private string GenerateFunctionBody<TCommandTree>(
            StorageAssociationSetModificationFunctionMapping modificationFunctionMapping,
            Func<ModificationCommandTreeGenerator, string, IEnumerable<TCommandTree>> treeGenerator,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator,
            string rowsAffectedParameterName)
            where TCommandTree : DbModificationCommandTree
        {
            DebugCheck.NotNull(modificationFunctionMapping);
            DebugCheck.NotNull(treeGenerator);

            var commandTrees = new TCommandTree[0];

            if (modificationCommandTreeGenerator != null)
            {
                var dynamicToFunctionModificationCommandConverter
                    = new DynamicToFunctionModificationCommandConverter(
                        modificationFunctionMapping,
                        _target.StorageEntityContainerMapping);

                commandTrees
                    = dynamicToFunctionModificationCommandConverter
                        .Convert(
                            treeGenerator(
                                modificationCommandTreeGenerator.Value,
                                modificationFunctionMapping.AssociationSet.ElementType.Identity))
                        .ToArray();
            }

            return GenerateFunctionBody(migrationSqlGenerator, rowsAffectedParameterName, commandTrees);
        }

        private string GenerateFunctionBody<TCommandTree>(
            MigrationSqlGenerator migrationSqlGenerator,
            string rowsAffectedParameterName,
            TCommandTree[] commandTrees)
            where TCommandTree : DbModificationCommandTree
        {
            if (migrationSqlGenerator == null)
            {
                return null;
            }

            var providerManifestToken = _target.ProviderInfo.ProviderManifestToken;

            return migrationSqlGenerator
                .GenerateProcedureBody(commandTrees, rowsAffectedParameterName, providerManifestToken);
        }

        private static bool DiffModificationFunction(
            StorageModificationFunctionMapping functionMapping1,
            StorageModificationFunctionMapping functionMapping2)
        {
            DebugCheck.NotNull(functionMapping1);
            DebugCheck.NotNull(functionMapping2);

            if (!functionMapping1.RowsAffectedParameterName.EqualsOrdinal(functionMapping2.RowsAffectedParameterName))
            {
                return false;
            }

            if (!functionMapping1.ParameterBindings
                     .SequenceEqual(
                         functionMapping2.ParameterBindings,
                         DiffParameterBinding))
            {
                return false;
            }

            var nullResultBindings
                = Enumerable.Empty<StorageModificationFunctionResultBinding>();

            if (!(functionMapping1.ResultBindings ?? nullResultBindings)
                     .SequenceEqual(
                         (functionMapping2.ResultBindings ?? nullResultBindings),
                         DiffResultBinding))
            {
                return false;
            }

            return true;
        }

        private static bool DiffParameterBinding(
            StorageModificationFunctionParameterBinding parameterBinding1,
            StorageModificationFunctionParameterBinding parameterBinding2)
        {
            DebugCheck.NotNull(parameterBinding1);
            DebugCheck.NotNull(parameterBinding2);

            if (!parameterBinding1.Parameter.Name.EqualsOrdinal(parameterBinding2.Parameter.Name))
            {
                return false;
            }

            if (parameterBinding1.IsCurrent != parameterBinding2.IsCurrent)
            {
                return false;
            }

            if (!parameterBinding1.MemberPath.Members
                     .SequenceEqual(
                         parameterBinding2.MemberPath.Members,
                         (m1, m2) => m1.Identity.EqualsOrdinal(m2.Identity)))
            {
                return false;
            }

            return true;
        }

        private static bool DiffResultBinding(
            StorageModificationFunctionResultBinding resultBinding1,
            StorageModificationFunctionResultBinding resultBinding2)
        {
            DebugCheck.NotNull(resultBinding1);
            DebugCheck.NotNull(resultBinding2);

            if (!resultBinding1.ColumnName.EqualsOrdinal(resultBinding2.ColumnName))
            {
                return false;
            }

            if (!resultBinding1.Property.Identity.EqualsOrdinal(resultBinding2.Property.Identity))
            {
                return false;
            }

            return true;
        }

        private IEnumerable<CreateProcedureOperation> BuildCreateProcedureOperations(
            StorageEntityTypeModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            yield return BuildCreateProcedureOperation(
                modificationFunctionMapping.InsertFunctionMapping.Function,
                GenerateInsertFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));

            yield return BuildCreateProcedureOperation(
                modificationFunctionMapping.UpdateFunctionMapping.Function,
                GenerateUpdateFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));

            yield return BuildCreateProcedureOperation(
                modificationFunctionMapping.DeleteFunctionMapping.Function,
                GenerateDeleteFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
        }

        private IEnumerable<CreateProcedureOperation> BuildCreateProcedureOperations(
            StorageAssociationSetModificationFunctionMapping modificationFunctionMapping,
            Lazy<ModificationCommandTreeGenerator> modificationCommandTreeGenerator,
            MigrationSqlGenerator migrationSqlGenerator)
        {
            DebugCheck.NotNull(modificationFunctionMapping);

            yield return BuildCreateProcedureOperation(
                modificationFunctionMapping.InsertFunctionMapping.Function,
                GenerateInsertFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));

            yield return BuildCreateProcedureOperation(
                modificationFunctionMapping.DeleteFunctionMapping.Function,
                GenerateDeleteFunctionBody(modificationFunctionMapping, modificationCommandTreeGenerator, migrationSqlGenerator));
        }

        private CreateProcedureOperation BuildCreateProcedureOperation(EdmFunction function, string bodySql)
        {
            DebugCheck.NotNull(function);

            var createProcedureOperation
                = new CreateProcedureOperation(GetSchemaQualifiedName(function.FunctionName, function.Schema), bodySql);

            function
                .Parameters
                .Each(p => createProcedureOperation.Parameters.Add(BuildParameterModel(p, _target)));

            return createProcedureOperation;
        }

        private AlterProcedureOperation BuildAlterProcedureOperation(EdmFunction function, string bodySql)
        {
            DebugCheck.NotNull(function);

            var alterProcedureOperation
                = new AlterProcedureOperation(GetSchemaQualifiedName(function.FunctionName, function.Schema), bodySql);

            function
                .Parameters
                .Each(p => alterProcedureOperation.Parameters.Add(BuildParameterModel(p, _target)));

            return alterProcedureOperation;
        }

        private static ParameterModel BuildParameterModel(
            FunctionParameter functionParameter,
            ModelMetadata modelMetadata)
        {
            DebugCheck.NotNull(functionParameter);
            DebugCheck.NotNull(modelMetadata);

            var edmTypeUsage
                = functionParameter.TypeUsage.ModelTypeUsage;

            var defaultStoreTypeName
                = modelMetadata.ProviderManifest.GetStoreType(edmTypeUsage).EdmType.Name;

            var parameterModel
                = new ParameterModel(((PrimitiveType)edmTypeUsage.EdmType).PrimitiveTypeKind, edmTypeUsage)
                    {
                        Name = functionParameter.Name,
                        IsOutParameter = functionParameter.Mode == ParameterMode.Out,
                        StoreType
                            = !functionParameter.TypeName.EqualsIgnoreCase(defaultStoreTypeName)
                                  ? functionParameter.TypeName
                                  : null
                    };

            Facet facet;

            if (edmTypeUsage.Facets.TryGetValue(DbProviderManifest.MaxLengthFacetName, true, out facet)
                && facet.Value != null)
            {
                parameterModel.MaxLength = facet.Value as int?; // could be MAX sentinel
            }

            if (edmTypeUsage.Facets.TryGetValue(DbProviderManifest.PrecisionFacetName, true, out facet)
                && facet.Value != null)
            {
                parameterModel.Precision = (byte?)facet.Value;
            }

            if (edmTypeUsage.Facets.TryGetValue(DbProviderManifest.ScaleFacetName, true, out facet)
                && facet.Value != null)
            {
                parameterModel.Scale = (byte?)facet.Value;
            }

            if (edmTypeUsage.Facets.TryGetValue(DbProviderManifest.FixedLengthFacetName, true, out facet)
                && facet.Value != null
                && (bool)facet.Value)
            {
                parameterModel.IsFixedLength = true;
            }

            if (edmTypeUsage.Facets.TryGetValue(DbProviderManifest.UnicodeFacetName, true, out facet)
                && facet.Value != null
                && !(bool)facet.Value)
            {
                parameterModel.IsUnicode = false;
            }

            return parameterModel;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<DropProcedureOperation> FindRemovedModificationFunctions()
        {
            return
                (from esm1 in _source.StorageEntityContainerMapping.EntitySetMappings
                 from mfm1 in esm1.ModificationFunctionMappings
                 where !(from esm2 in _target.StorageEntityContainerMapping.EntitySetMappings
                         from mfm2 in esm2.ModificationFunctionMappings
                         where mfm1.EntityType.Identity == mfm2.EntityType.Identity
                         select mfm2
                        ).Any()
                 from o in new[]
                     {
                         new DropProcedureOperation(
                     GetSchemaQualifiedName(
                         mfm1.InsertFunctionMapping.Function.FunctionName,
                         mfm1.InsertFunctionMapping.Function.Schema)),
                         new DropProcedureOperation(
                     GetSchemaQualifiedName(
                         mfm1.UpdateFunctionMapping.Function.FunctionName,
                         mfm1.UpdateFunctionMapping.Function.Schema)),
                         new DropProcedureOperation(
                     GetSchemaQualifiedName(
                         mfm1.DeleteFunctionMapping.Function.FunctionName,
                         mfm1.DeleteFunctionMapping.Function.Schema))
                     }
                 select o)
                    .Concat(
                        from asm1 in _source.StorageEntityContainerMapping.AssociationSetMappings
                        where asm1.ModificationFunctionMapping != null
                        where !(from asm2 in _target.StorageEntityContainerMapping.AssociationSetMappings
                                where asm2.ModificationFunctionMapping != null
                                      && asm1.ModificationFunctionMapping.AssociationSet.Identity
                                      == asm2.ModificationFunctionMapping.AssociationSet.Identity
                                select asm2.ModificationFunctionMapping
                               ).Any()
                        from o in new[]
                            {
                                new DropProcedureOperation(
                            GetSchemaQualifiedName(
                                asm1.ModificationFunctionMapping.InsertFunctionMapping.Function.FunctionName,
                                asm1.ModificationFunctionMapping.InsertFunctionMapping.Function.Schema)),
                                new DropProcedureOperation(
                            GetSchemaQualifiedName(
                                asm1.ModificationFunctionMapping.DeleteFunctionMapping.Function.FunctionName,
                                asm1.ModificationFunctionMapping.DeleteFunctionMapping.Function.Schema))
                            }
                        select o);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<Tuple<EntitySet, EntitySet>> FindTablePairs()
        {
            // Zip the two models together. Our goal here is to match tables across
            // the source and target input models.

            return
                (from esm1 in _source.StorageEntityContainerMapping.EntitySetMappings
                 from esm2 in _target.StorageEntityContainerMapping.EntitySetMappings
                 where esm1.EntitySet.ElementType.Name.EqualsIgnoreCase(esm2.EntitySet.ElementType.Name)
                 from etm1 in esm1.EntityTypeMappings
                 from etm2 in esm2.EntityTypeMappings
                 where (etm1.EntityType != null
                        && etm2.EntityType != null
                        && etm1.EntityType.Name.EqualsIgnoreCase(etm2.EntityType.Name))
                       || (etm1.EntityType == null
                           && etm2.EntityType == null
                           && etm1.IsOfTypes.Select(t => t.Name).SequenceEqual(etm2.IsOfTypes.Select(t => t.Name)))
                 from mfs in etm1.MappingFragments.Zip(etm2.MappingFragments)
                 select Tuple.Create(mfs.Key.TableSet, mfs.Value.TableSet))
                    .Concat(
                        from et1 in _source.EdmItemCollection.GetItems<EntityType>()
                        from et2 in _target.EdmItemCollection.GetItems<EntityType>()
                        where et1.Name.EqualsIgnoreCase(et2.Name)
                        from np1 in et1.NavigationProperties
                        from np2 in et2.NavigationProperties
                        where np1.Name.EqualsIgnoreCase(np2.Name)
                        from asm1 in _source.StorageEntityContainerMapping.AssociationSetMappings
                        from asm2 in _target.StorageEntityContainerMapping.AssociationSetMappings
                        where asm1.AssociationSet.ElementType == np1.Association
                              && asm2.AssociationSet.ElementType == np2.Association
                        select Tuple.Create(asm1.StoreEntitySet, asm2.StoreEntitySet));
        }

        private static IEnumerable<RenameTableOperation> FindRenamedTables(ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
        {
            DebugCheck.NotNull(tablePairs);

            return (from p in tablePairs
                    where !p.Item1.Table.EqualsIgnoreCase(p.Item2.Table)
                    select
                        new RenameTableOperation(
                            GetSchemaQualifiedName(p.Item1.Table, p.Item1.Schema), p.Item2.Table))
                .Distinct(
                    new DynamicEqualityComparer<RenameTableOperation>(
                        (r1, r2) => r1.Name.EqualsIgnoreCase(r2.Name) && r1.NewName.EqualsIgnoreCase(r2.NewName)));
        }

        private IEnumerable<CreateTableOperation> FindAddedTables(
            ICollection<RenameTableOperation> renamedTables, ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
        {
            DebugCheck.NotNull(renamedTables);
            DebugCheck.NotNull(tablePairs);

            return (_target.StoreItemCollection.GetItems<EntityContainer>().Single().EntitySets
                .Except(
                    tablePairs.Select(p => p.Item2),
                    (x1, x2) => x1.Name.EqualsIgnoreCase(x2.Name))
                .Where(es => !renamedTables.Any(rt => rt.NewName.EqualsIgnoreCase(es.Table)))
                .Select(es => BuildCreateTableOperation(es.Name, es.Table, es.Schema, _target)))
                .Distinct(new DynamicEqualityComparer<CreateTableOperation>((t1, t2) => t1.Name.EqualsIgnoreCase(t2.Name)));
        }

        private IEnumerable<MoveTableOperation> FindMovedTables(ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
        {
            DebugCheck.NotNull(tablePairs);

            return (from p in tablePairs
                    where !p.Item1.Schema.EqualsIgnoreCase(p.Item2.Schema)
                    select
                        new MoveTableOperation(
                            GetSchemaQualifiedName(p.Item2.Table, p.Item1.Schema), p.Item2.Schema)
                            {
                                CreateTableOperation =
                                    BuildCreateTableOperation(
                                        p.Item2.Name, p.Item2.Table, p.Item2.Schema, _target)
                            })
                .Distinct(
                    new DynamicEqualityComparer<MoveTableOperation>(
                        (m1, m2) => m1.Name.EqualsIgnoreCase(m2.Name) && m1.NewSchema.EqualsIgnoreCase(m2.NewSchema)));
        }

        private IEnumerable<DropTableOperation> FindRemovedTables(
            ICollection<RenameTableOperation> renamedTables, ICollection<Tuple<EntitySet, EntitySet>> tablePairs)
        {
            DebugCheck.NotNull(renamedTables);
            DebugCheck.NotNull(tablePairs);

            return
                (_source.StoreItemCollection.GetItems<EntityContainer>().Single().EntitySets
                    .Except(
                        tablePairs.Select(p => p.Item1),
                        (x1, x2) => x1.Name.EqualsIgnoreCase(x2.Name))
                    .Where(es => !renamedTables.Any(rt => rt.Name.EqualsIgnoreCase(es.Table)))
                    .Select(
                        es => new DropTableOperation(
                            GetSchemaQualifiedName(es.Table, es.Schema),
                            BuildCreateTableOperation(
                                es.Name,
                                es.Table,
                                es.Schema,
                                _source)))).Distinct(
                                    new DynamicEqualityComparer<DropTableOperation>(
                                        (t1, t2) => t1.Name.EqualsIgnoreCase(t2.Name)));
        }

        private IEnumerable<DropColumnOperation> FindRemovedColumns(ICollection<RenameColumnOperation> renamedColumns)
        {
            DebugCheck.NotNull(renamedColumns);

            return from t1 in _source.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   from t2 in _target.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   where t1.NameAttribute().EqualsIgnoreCase(t2.NameAttribute())
                   let t = GetQualifiedTableName(_target.Model, t2.NameAttribute())
                   from c in t1.Descendants(EdmXNames.Ssdl.PropertyNames)
                       .Except(
                           t2.Descendants(EdmXNames.Ssdl.PropertyNames),
                           (c1, c2) => c1.NameAttribute().EqualsIgnoreCase(c2.NameAttribute()))
                   where !renamedColumns.Any(rc => rc.Name.EqualsIgnoreCase(c.NameAttribute()))
                   select new DropColumnOperation(
                       t,
                       c.NameAttribute(),
                       new AddColumnOperation(t, BuildColumnModel(c, t1.NameAttribute(), _source)));
        }

        private IEnumerable<DropColumnOperation> FindOrphanedColumns(ICollection<RenameColumnOperation> renamedColumns)
        {
            DebugCheck.NotNull(renamedColumns);

            return from rc1 in renamedColumns
                   from et in _source.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   let t = GetQualifiedTableName(_source.Model, et.NameAttribute())
                   where rc1.Table.EqualsIgnoreCase(t)
                         && _source.Model
                                .Descendants(EdmXNames.Msl.AssociationSetMappingNames)
                                .Where(asm => asm.StoreEntitySetAttribute().EqualsIgnoreCase(et.NameAttribute()))
                                .Descendants(EdmXNames.Msl.ScalarPropertyNames)
                                .Any(sp => sp.ColumnNameAttribute().EqualsIgnoreCase(rc1.Name))
                         && !renamedColumns.Any(
                             // Ensure the candidate column is not also being renamed
                             rc2 =>
                             rc2 != rc1
                             && rc2.Table.EqualsIgnoreCase(rc1.Table)
                             && rc2.Name.EqualsIgnoreCase(rc1.NewName))
                   let oc = et.Descendants(EdmXNames.Ssdl.PropertyNames)
                       .SingleOrDefault(c => rc1.NewName.EqualsIgnoreCase(c.NameAttribute()))
                   where oc != null
                   select new DropColumnOperation(
                       t,
                       oc.NameAttribute(),
                       new AddColumnOperation(t, BuildColumnModel(oc, et.NameAttribute(), _source)));
        }

        private IEnumerable<RenameColumnOperation> FindRenamedColumns()
        {
            return
                FindRenamedMappedColumns()
                    .Concat(FindRenamedForeignKeyColumns())
                    .Concat(FindRenamedDiscriminatorColumns())
                    .Distinct(
                        new DynamicEqualityComparer<RenameColumnOperation>(
                            (c1, c2) => c1.Table.EqualsIgnoreCase(c2.Table)
                                        && c1.Name.EqualsIgnoreCase(c2.Name)
                                        && c1.NewName.EqualsIgnoreCase(c2.NewName)));
        }

        private IEnumerable<RenameColumnOperation> FindRenamedMappedColumns()
        {
            return from etm1 in _source.Model.Descendants(EdmXNames.Msl.EntityTypeMappingNames)
                   from etm2 in _target.Model.Descendants(EdmXNames.Msl.EntityTypeMappingNames)
                   where etm1.TypeNameAttribute().EqualsIgnoreCase(etm2.TypeNameAttribute())
                   from mf1 in etm1.Descendants(EdmXNames.Msl.MappingFragmentNames)
                   from mf2 in etm2.Descendants(EdmXNames.Msl.MappingFragmentNames)
                   where mf1.StoreEntitySetAttribute().EqualsIgnoreCase(mf2.StoreEntitySetAttribute())
                   let t = GetQualifiedTableName(_target.Model, mf1.StoreEntitySetAttribute())
                   from cr in FindRenamedMappedColumns(mf1, mf2, t)
                   select cr;
        }

        private static IEnumerable<RenameColumnOperation> FindRenamedMappedColumns(
            XElement parent1, XElement parent2, string table)
        {
            DebugCheck.NotNull(parent1);
            DebugCheck.NotNull(parent2);
            DebugCheck.NotEmpty(table);

            return (from p1 in parent1.Elements(EdmXNames.Msl.ScalarPropertyNames)
                    from p2 in parent2.Elements(EdmXNames.Msl.ScalarPropertyNames)
                    where p1.NameAttribute().EqualsIgnoreCase(p2.NameAttribute())
                    where !p1.ColumnNameAttribute().EqualsIgnoreCase(p2.ColumnNameAttribute())
                    select new RenameColumnOperation(table, p1.ColumnNameAttribute(), p2.ColumnNameAttribute()))
                .Concat(
                    from p1 in parent1.Elements(EdmXNames.Msl.ComplexPropertyNames)
                    from p2 in parent2.Elements(EdmXNames.Msl.ComplexPropertyNames)
                    where p1.NameAttribute().EqualsIgnoreCase(p2.NameAttribute())
                    from cr in FindRenamedMappedColumns(p1, p2, table)
                    select cr);
        }

        private IEnumerable<RenameColumnOperation> FindRenamedDiscriminatorColumns()
        {
            return from etm1 in _source.Model.Descendants(EdmXNames.Msl.EntityTypeMappingNames)
                   from etm2 in _target.Model.Descendants(EdmXNames.Msl.EntityTypeMappingNames)
                   where etm1.TypeNameAttribute().EqualsIgnoreCase(etm2.TypeNameAttribute())
                   from mf1 in etm1.Descendants(EdmXNames.Msl.MappingFragmentNames)
                   from mf2 in etm2.Descendants(EdmXNames.Msl.MappingFragmentNames)
                   where mf1.StoreEntitySetAttribute().EqualsIgnoreCase(mf2.StoreEntitySetAttribute())
                   let t = GetQualifiedTableName(_target.Model, mf2.StoreEntitySetAttribute())
                   from cr in FindRenamedDiscriminatorColumns(mf1, mf2, t)
                   select cr;
        }

        private static IEnumerable<RenameColumnOperation> FindRenamedDiscriminatorColumns(
            XElement parent1, XElement parent2, string table)
        {
            DebugCheck.NotNull(parent1);
            DebugCheck.NotNull(parent2);
            DebugCheck.NotEmpty(table);

            return from p1 in parent1.Elements(EdmXNames.Msl.ConditionNames)
                   from p2 in parent2.Elements(EdmXNames.Msl.ConditionNames)
                   where p1.ValueAttribute().EqualsIgnoreCase(p2.ValueAttribute())
                   where !p1.ColumnNameAttribute().EqualsIgnoreCase(p2.ColumnNameAttribute())
                   select new RenameColumnOperation(table, p1.ColumnNameAttribute(), p2.ColumnNameAttribute());
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<RenameColumnOperation> FindRenamedForeignKeyColumns()
        {
            return from a1 in _source.Model.Descendants(EdmXNames.Ssdl.AssociationNames)
                   from a2 in _target.Model.Descendants(EdmXNames.Ssdl.AssociationNames)
                   where a1.NameAttribute().EqualsIgnoreCase(a2.NameAttribute())
                   let d1 = a1.Descendants(EdmXNames.Ssdl.DependentNames).Single()
                   let d2 = a2.Descendants(EdmXNames.Ssdl.DependentNames).Single()
                   where d1.RoleAttribute().EqualsIgnoreCase(d2.RoleAttribute())
                   from n1 in d1.Descendants(EdmXNames.Ssdl.PropertyRefNames)
                       .Select(x => x.NameAttribute()).Select(
                           (name, index) => new
                               {
                                   name,
                                   index
                               })
                   from n2 in d2.Descendants(EdmXNames.Ssdl.PropertyRefNames)
                       .Select(x => x.NameAttribute()).Select(
                           (name, index) => new
                               {
                                   name,
                                   index
                               })
                   where (n1.index == n2.index)
                         && !n1.name.EqualsIgnoreCase(n2.name)
                   let es = a2.Descendants(EdmXNames.Ssdl.EndNames)
                       .Single(e => e.RoleAttribute().EqualsOrdinal(d2.RoleAttribute()))
                       .TypeAttribute().Split(new[] { '.' }).Last()
                   where !_target.Model
                              .Descendants(EdmXNames.Ssdl.EntityTypeNames)
                              .Single(et => et.NameAttribute().EqualsIgnoreCase(es))
                              .Descendants(EdmXNames.Ssdl.PropertyNames)
                              .Any(p => p.NameAttribute().EqualsIgnoreCase(n1.name))
                         || (from a3 in _target.Model.Descendants(EdmXNames.Ssdl.AssociationNames)
                             where !a2.NameAttribute().EqualsIgnoreCase(a3.NameAttribute())
                             let d3 = a3.Descendants(EdmXNames.Ssdl.DependentNames).Single()
                             where d2.RoleAttribute().EqualsIgnoreCase(d3.RoleAttribute())
                             from n3 in d3.Descendants(EdmXNames.Ssdl.PropertyRefNames)
                             where n3.NameAttribute().EqualsIgnoreCase(n1.name)
                             select n3)
                                .Any()
                   let t = GetQualifiedTableName(_target.Model, es)
                   select new RenameColumnOperation(t, n1.name, n2.name);
        }

        private IEnumerable<AddColumnOperation> FindAddedColumns(ICollection<RenameColumnOperation> renamedColumns)
        {
            DebugCheck.NotNull(renamedColumns);

            return from t1 in _source.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   from t2 in _target.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   where t1.NameAttribute().EqualsIgnoreCase(t2.NameAttribute())
                   let t = GetQualifiedTableName(_target.Model, t2.NameAttribute())
                   from p2 in t2.Descendants(EdmXNames.Ssdl.PropertyNames)
                   let columnName = p2.NameAttribute()
                   where !t1.Descendants(EdmXNames.Ssdl.PropertyNames)
                              .Any(p1 => columnName.EqualsIgnoreCase(p1.NameAttribute()))
                         && !renamedColumns
                                 .Any(cr => cr.Table.EqualsIgnoreCase(t) && cr.NewName.EqualsIgnoreCase(columnName))
                   select new AddColumnOperation(t, BuildColumnModel(p2, t2.NameAttribute(), _target));
        }

        private IEnumerable<AlterColumnOperation> FindChangedColumns(ICollection<RenameColumnOperation> renamedColumns)
        {
            DebugCheck.NotNull(renamedColumns);

            return from t1 in _source.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   from t2 in _target.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   where t1.NameAttribute().EqualsIgnoreCase(t2.NameAttribute())
                   let t = GetQualifiedTableName(_target.Model, t2.NameAttribute())
                   from p1 in t1.Descendants(EdmXNames.Ssdl.PropertyNames)
                   from p2 in t2.Descendants(EdmXNames.Ssdl.PropertyNames)
                   let rc = renamedColumns
                       .SingleOrDefault(
                           rc => rc.Table.EqualsIgnoreCase(t)
                                 && rc.Name.EqualsIgnoreCase(p1.NameAttribute())
                                 && rc.NewName.EqualsIgnoreCase(p2.NameAttribute()))
                   where (rc != null
                          || p1.NameAttribute().EqualsIgnoreCase(p2.NameAttribute()))
                         && !DiffColumns(p1, p2, rc)
                   select BuildAlterColumnOperation(t, p2, t2.NameAttribute(), _target, p1, t1.NameAttribute(), _source);
        }

        private bool DiffColumns(XElement column1, XElement column2, RenameColumnOperation renameColumnOperation)
        {
            DebugCheck.NotNull(column1);
            DebugCheck.NotNull(column2);

            if (renameColumnOperation != null)
            {
                // normalize if column is being renamed
                column1 = new XElement(column1);
                column1.SetAttributeValue("Name", renameColumnOperation.NewName);
            }

            if (_consistentProviders)
            {
                return CanonicalDeepEquals(column1, column2);
            }

            var c1 = new XElement(column1); // clone
            var c2 = new XElement(column2);

            c1.SetAttributeValue("Type", null);
            c2.SetAttributeValue("Type", null);

            if (((c1.MaxLengthAttribute() != null) && (c2.MaxLengthAttribute() == null))
                || ((c1.MaxLengthAttribute() == null) && (c2.MaxLengthAttribute() != null)))
            {
                c1.SetAttributeValue("MaxLength", null);
                c2.SetAttributeValue("MaxLength", null);
            }

            return CanonicalDeepEquals(c1, c2);
        }

        private AlterColumnOperation BuildAlterColumnOperation(
            string table,
            XElement targetProperty,
            string targetEntitySetName,
            ModelMetadata targetModelMetadata,
            XElement sourceProperty,
            string sourceEntitySetName,
            ModelMetadata sourceModelMetadata)
        {
            var targetModel = BuildColumnModel(targetProperty, targetEntitySetName, targetModelMetadata);
            var sourceModel = BuildColumnModel(sourceProperty, sourceEntitySetName, sourceModelMetadata);

            // In-case the column is also being renamed.
            sourceModel.Name = targetModel.Name;

            return new AlterColumnOperation(
                table,
                targetModel,
                isDestructiveChange: targetModel.IsNarrowerThan(sourceModel, _target.ProviderManifest),
                inverse: new AlterColumnOperation(
                    table,
                    sourceModel,
                    isDestructiveChange: sourceModel.IsNarrowerThan(targetModel, _target.ProviderManifest)));
        }

        private static bool CanonicalDeepEquals(XElement element1, XElement element2)
        {
            var canonical1 = Canonicalize(element1);
            var canonical2 = Canonicalize(element2);

            return XNode.DeepEquals(canonical1, canonical2);
        }

        private static XElement Canonicalize(XElement element)
        {
            if (element == null)
            {
                return null;
            }

            var canonical = new XElement(element);

            foreach (var e in canonical.DescendantsAndSelf())
            {
                if (e.Name.Namespace
                    != XNamespace.None)
                {
                    e.Name = XNamespace.None.GetName(e.Name.LocalName);
                }

                e.ReplaceAttributes(
                    e.Attributes()
                        .Select(
                            a => a.IsNamespaceDeclaration
                                     ? null
                                     : (a.Name.Namespace != XNamespace.None)
                                           ? new XAttribute(XNamespace.None.GetName(a.Name.LocalName), a.Value)
                                           : a)
                        .OrderBy(a => a.Name.LocalName));
            }

            return canonical;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private IEnumerable<Tuple<XElement, XElement>> FindAssociationPairs(XDocument columnNormalizedSourceModel)
        {
            DebugCheck.NotNull(columnNormalizedSourceModel);

            var sourceEntityTypes = columnNormalizedSourceModel.Descendants(EdmXNames.Csdl.EntityTypeNames).Select(et => new { Xml = et, Name = et.NameAttribute(), NavigationProperties = et.Descendants(EdmXNames.Csdl.NavigationPropertyNames).Select(np => new { Xml = np, Name = np.NameAttribute() }).ToList() }).ToList();
            var targetEntityTypes = _target.Model.Descendants(EdmXNames.Csdl.EntityTypeNames).Select(et => new { Xml = et, Name = et.NameAttribute(), NavigationProperties = et.Descendants(EdmXNames.Csdl.NavigationPropertyNames).Select(np => new { Xml = np, Name = np.NameAttribute() }).ToList()}).ToList();
            var sourceAssociations = columnNormalizedSourceModel.Descendants(EdmXNames.Ssdl.AssociationNames).Select(a => new { Xml = a, Name = a.NameAttribute() }).ToList();
            var targetAssociations = _target.Model.Descendants(EdmXNames.Ssdl.AssociationNames).Select(a => new { Xml = a, Name = a.NameAttribute() }).ToList();

            return (from et1 in sourceEntityTypes
                    from et2 in targetEntityTypes
                    where string.Equals(et1.Name, et2.Name, StringComparison.OrdinalIgnoreCase)
                    from np1 in et1.NavigationProperties
                    from np2 in et2.NavigationProperties
                    where string.Equals(np1.Name, np2.Name, StringComparison.OrdinalIgnoreCase)
                    let associations =
                        new
                            {
                                SourceAssociation = np1.Xml.RelationshipAttribute().Split(new[] { '.' }).Last(),
                                SourceFromRole = np1.Xml.FromRoleAttribute(),
                                SourceToRole = np1.Xml.ToRoleAttribute(),
                                TargetAssociation = np2.Xml.RelationshipAttribute().Split(new[] { '.' }).Last(),
                                TargetFromRole = np2.Xml.FromRoleAttribute(),
                                TargetToRole = np2.Xml.ToRoleAttribute()
                            }
                    from a1 in sourceAssociations
                    from a2 in targetAssociations
                    where
                        (string.Equals(a1.Name, associations.SourceAssociation, StringComparison.OrdinalIgnoreCase)
                         && string.Equals(a2.Name, associations.TargetAssociation, StringComparison.OrdinalIgnoreCase))
                        || (string.Equals(a1.Name, associations.SourceFromRole, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(a2.Name, associations.TargetFromRole, StringComparison.OrdinalIgnoreCase))
                        || (string.Equals(a1.Name, associations.SourceToRole, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(a2.Name, associations.TargetToRole, StringComparison.OrdinalIgnoreCase))
                        || string.Equals(a1.Name, a2.Name, StringComparison.OrdinalIgnoreCase)
                    select Tuple.Create(a1.Xml, a2.Xml)).Distinct(
                        (t1, t2) =>
                            t1.Item1.NameAttribute() == t2.Item1.NameAttribute()
                            && t1.Item2.NameAttribute() == t2.Item2.NameAttribute());
        }

        private IEnumerable<AddForeignKeyOperation> FindAddedForeignKeys(ICollection<Tuple<XElement, XElement>> assocationPairs)
        {
            DebugCheck.NotNull(assocationPairs);

            return _target.Model.Descendants(EdmXNames.Ssdl.AssociationNames)
                .Except(assocationPairs.Select(p => p.Item2), (x1, x2) => x1.NameAttribute().EqualsIgnoreCase(x2.NameAttribute()))
                .Concat(assocationPairs.Where(fk => !DiffAssociations(fk.Item1, fk.Item2)).Select(fk => fk.Item2))
                .Select(a => BuildAddForeignKeyOperation(_target.Model, a));
        }

        private IEnumerable<DropForeignKeyOperation> FindRemovedForeignKeys(
            XDocument columnNormalizedSourceModel, ICollection<Tuple<XElement, XElement>> assocationPairs)
        {
            DebugCheck.NotNull(columnNormalizedSourceModel);
            DebugCheck.NotNull(assocationPairs);
            
            return columnNormalizedSourceModel.Descendants(EdmXNames.Ssdl.AssociationNames)
                .Except(assocationPairs.Select(p => p.Item1), (x1, x2) => x1.NameAttribute().EqualsIgnoreCase(x2.NameAttribute()))
                .Concat(assocationPairs.Where(fk => !DiffAssociations(fk.Item1, fk.Item2)).Select(fk => fk.Item1))
                .Select(
                    a =>
                    BuildDropForeignKeyOperation(
                        _source.Model,
                        _source.Model.Descendants(EdmXNames.Ssdl.AssociationNames)
                        .Single(a2 => a2.NameAttribute().EqualsIgnoreCase(a.NameAttribute()))));
        }

        private static bool DiffAssociations(XElement a1, XElement a2)
        {
            DebugCheck.NotNull(a1);
            DebugCheck.NotNull(a2);

            var principal1 = a1.Descendants(EdmXNames.Ssdl.PrincipalNames).Single();
            var principal2 = a2.Descendants(EdmXNames.Ssdl.PrincipalNames).Single();

            if (
                !principal1.Descendants(EdmXNames.Ssdl.PropertyRefNames)
                     .Select(pr => pr.NameAttribute())
                     .SequenceEqual(principal2.Descendants(EdmXNames.Ssdl.PropertyRefNames).Select(pr => pr.NameAttribute())))
            {
                return false;
            }

            var principalEnd1 =
                a1.Descendants(EdmXNames.Ssdl.EndNames).Single(e => e.RoleAttribute().EqualsIgnoreCase(principal1.RoleAttribute()));
            var principalEnd2 =
                a2.Descendants(EdmXNames.Ssdl.EndNames).Single(e => e.RoleAttribute().EqualsIgnoreCase(principal2.RoleAttribute()));

            if (!principalEnd1.MultiplicityAttribute().EqualsIgnoreCase(principalEnd2.MultiplicityAttribute()))
            {
                return false;
            }

            if (
                !CanonicalDeepEquals(
                    principalEnd1.Descendants(EdmXNames.Ssdl.OnDeleteNames).SingleOrDefault(),
                    principalEnd2.Descendants(EdmXNames.Ssdl.OnDeleteNames).SingleOrDefault()))
            {
                return false;
            }

            var dependent1 = a1.Descendants(EdmXNames.Ssdl.DependentNames).Single();
            var dependent2 = a2.Descendants(EdmXNames.Ssdl.DependentNames).Single();

            if (
                !dependent1.Descendants(EdmXNames.Ssdl.PropertyRefNames)
                     .Select(pr => pr.NameAttribute())
                     .SequenceEqual(dependent2.Descendants(EdmXNames.Ssdl.PropertyRefNames).Select(pr => pr.NameAttribute())))
            {
                return false;
            }

            var dependentEnd1 =
                a1.Descendants(EdmXNames.Ssdl.EndNames).Single(e => e.RoleAttribute().EqualsIgnoreCase(dependent1.RoleAttribute()));
            var dependentEnd2 =
                a2.Descendants(EdmXNames.Ssdl.EndNames).Single(e => e.RoleAttribute().EqualsIgnoreCase(dependent2.RoleAttribute()));

            if (!dependentEnd1.MultiplicityAttribute().EqualsIgnoreCase(dependentEnd2.MultiplicityAttribute()))
            {
                return false;
            }

            return CanonicalDeepEquals(
                dependentEnd1.Descendants(EdmXNames.Ssdl.OnDeleteNames).SingleOrDefault(),
                dependentEnd2.Descendants(EdmXNames.Ssdl.OnDeleteNames).SingleOrDefault());
        }

        private IEnumerable<PrimaryKeyOperation> FindChangedPrimaryKeys(XDocument columnNormalizedSourceModel)
        {
            DebugCheck.NotNull(columnNormalizedSourceModel);

            return from et1 in columnNormalizedSourceModel.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   from et2 in _target.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                   where et1.NameAttribute().EqualsIgnoreCase(et2.NameAttribute())
                   where !CanonicalDeepEquals(
                       et1.Descendants(EdmXNames.Ssdl.KeyNames).Single(),
                       et2.Descendants(EdmXNames.Ssdl.KeyNames).Single())
                   from pko in BuildChangePrimaryKeyOperations(
                       GetQualifiedTableName(_source.Model, et1.NameAttribute()),
                       GetQualifiedTableName(_target.Model, et2.NameAttribute()),
                       _source.Model
                       .Descendants(EdmXNames.Ssdl.EntityTypeNames)
                       .Single(et => et.NameAttribute().EqualsIgnoreCase(et1.NameAttribute()))
                       .Descendants(EdmXNames.Ssdl.KeyNames).Single(),
                       _target.Model
                       .Descendants(EdmXNames.Ssdl.EntityTypeNames)
                       .Single(et => et.NameAttribute().EqualsIgnoreCase(et1.NameAttribute()))
                       .Descendants(EdmXNames.Ssdl.KeyNames).Single())
                   select pko;
        }

        private static IEnumerable<PrimaryKeyOperation> BuildChangePrimaryKeyOperations(
            string oldTable, string newTable, XElement oldKey, XElement newKey)
        {
            var dropPrimaryKeyOperation
                = new DropPrimaryKeyOperation
                    {
                        Table = oldTable
                    };

            oldKey.Descendants(EdmXNames.Ssdl.PropertyRefNames).Each(
                pr => dropPrimaryKeyOperation.Columns.Add(pr.NameAttribute()));

            yield return dropPrimaryKeyOperation;

            var addPrimaryKeyOperation
                = new AddPrimaryKeyOperation
                    {
                        Table = newTable
                    };

            newKey.Descendants(EdmXNames.Ssdl.PropertyRefNames).Each(
                pr => addPrimaryKeyOperation.Columns.Add(pr.NameAttribute()));

            yield return addPrimaryKeyOperation;
        }

        private static CreateTableOperation BuildCreateTableOperation(
            string entitySetName,
            string tableName,
            string schema,
            ModelMetadata modelMetadata)
        {
            DebugCheck.NotEmpty(entitySetName);
            DebugCheck.NotEmpty(tableName);
            DebugCheck.NotEmpty(schema);
            DebugCheck.NotNull(modelMetadata);

            var createTableOperation = new CreateTableOperation(GetSchemaQualifiedName(tableName, schema));

            var entityTypeElement = modelMetadata.Model.Descendants(EdmXNames.Ssdl.EntityTypeNames)
                .Single(et => et.NameAttribute().EqualsIgnoreCase(entitySetName));

            entityTypeElement
                .Descendants(EdmXNames.Ssdl.PropertyNames)
                .Each(p => createTableOperation.Columns.Add(BuildColumnModel(p, entitySetName, modelMetadata)));

            var addPrimaryKeyOperation = new AddPrimaryKeyOperation();

            entityTypeElement
                .Descendants(EdmXNames.Ssdl.PropertyRefNames)
                .Each(pr => addPrimaryKeyOperation.Columns.Add(pr.NameAttribute()));

            createTableOperation.PrimaryKey = addPrimaryKeyOperation;

            return createTableOperation;
        }

        private static ColumnModel BuildColumnModel(
            XElement property, string entitySetName, ModelMetadata modelMetadata)
        {
            DebugCheck.NotNull(property);
            DebugCheck.NotEmpty(entitySetName);
            DebugCheck.NotNull(modelMetadata);

            var nameAttribute = property.NameAttribute();
            var nullableAttribute = property.NullableAttribute();
            var maxLengthAttribute = property.MaxLengthAttribute();
            var precisionAttribute = property.PrecisionAttribute();
            var scaleAttribute = property.ScaleAttribute();
            var storeGeneratedPatternAttribute = property.StoreGeneratedPatternAttribute();
            var storeType = property.TypeAttribute();

            var entityType
                = modelMetadata.StoreItemCollection
                    .OfType<EntityType>()
                    .Single(et => et.Name.EqualsIgnoreCase(entitySetName));

            var edmProperty
                = entityType.Properties[nameAttribute];

            var typeUsage = modelMetadata.ProviderManifest.GetEdmType(edmProperty.TypeUsage);

            var defaultStoreTypeName = modelMetadata.ProviderManifest.GetStoreType(typeUsage).EdmType.Name;

            var column
                = new ColumnModel(((PrimitiveType)edmProperty.TypeUsage.EdmType).PrimitiveTypeKind, typeUsage)
                    {
                        Name = nameAttribute,
                        IsNullable
                            = !string.IsNullOrWhiteSpace(nullableAttribute)
                              && !Convert.ToBoolean(nullableAttribute, CultureInfo.InvariantCulture)
                                  ? false
                                  : (bool?)null,
                        MaxLength
                            // Setting "Max" is equivalent to not setting anything
                            = !string.IsNullOrWhiteSpace(maxLengthAttribute) && !maxLengthAttribute.EqualsIgnoreCase(XmlConstants.Max)
                                  ? Convert.ToInt32(maxLengthAttribute, CultureInfo.InvariantCulture)
                                  : (int?)null,
                        Precision
                            = !string.IsNullOrWhiteSpace(precisionAttribute)
                                  ? Convert.ToByte(precisionAttribute, CultureInfo.InvariantCulture)
                                  : (byte?)null,
                        Scale
                            = !string.IsNullOrWhiteSpace(scaleAttribute)
                                  ? Convert.ToByte(scaleAttribute, CultureInfo.InvariantCulture)
                                  : (byte?)null,
                        StoreType
                            = !storeType.EqualsIgnoreCase(defaultStoreTypeName)
                                  ? storeType
                                  : null
                    };

            column.IsIdentity
                = !string.IsNullOrWhiteSpace(storeGeneratedPatternAttribute)
                  && storeGeneratedPatternAttribute.EqualsIgnoreCase("Identity")
                  && _validIdentityTypes.Contains(column.Type);

            Facet facet;
            if (typeUsage.Facets.TryGetValue(DbProviderManifest.FixedLengthFacetName, true, out facet)
                && facet.Value != null
                && (bool)facet.Value)
            {
                column.IsFixedLength = true;
            }

            if (typeUsage.Facets.TryGetValue(DbProviderManifest.UnicodeFacetName, true, out facet)
                && facet.Value != null
                && !(bool)facet.Value)
            {
                column.IsUnicode = false;
            }

            var isComputed
                = !string.IsNullOrWhiteSpace(storeGeneratedPatternAttribute)
                  && storeGeneratedPatternAttribute.EqualsIgnoreCase("Computed");

            if ((column.Type == PrimitiveTypeKind.Binary)
                && (typeUsage.Facets.TryGetValue(DbProviderManifest.MaxLengthFacetName, true, out facet)
                    && (facet.Value is int)
                    && ((int)facet.Value == 8))
                && isComputed)
            {
                column.IsTimestamp = true;
            }

            return column;
        }

        private static AddForeignKeyOperation BuildAddForeignKeyOperation(XDocument edmx, XElement association)
        {
            DebugCheck.NotNull(edmx);
            DebugCheck.NotNull(association);

            var addForeignKeyOperation = new AddForeignKeyOperation();

            BuildForeignKeyOperation(edmx, association, addForeignKeyOperation);

            var principal = association.Descendants(EdmXNames.Ssdl.PrincipalNames).Single();

            principal.Descendants(EdmXNames.Ssdl.PropertyRefNames)
                .Each(pr => addForeignKeyOperation.PrincipalColumns.Add(pr.NameAttribute()));

            var onDelete = association.Descendants(EdmXNames.Ssdl.OnDeleteNames).SingleOrDefault();

            if ((onDelete != null)
                && onDelete.ActionAttribute().EqualsIgnoreCase("Cascade"))
            {
                addForeignKeyOperation.CascadeDelete = true;
            }

            return addForeignKeyOperation;
        }

        private static DropForeignKeyOperation BuildDropForeignKeyOperation(XDocument edmx, XElement association)
        {
            DebugCheck.NotNull(edmx);
            DebugCheck.NotNull(association);

            var dropForeignKeyOperation
                = new DropForeignKeyOperation(BuildAddForeignKeyOperation(edmx, association));

            BuildForeignKeyOperation(edmx, association, dropForeignKeyOperation);

            return dropForeignKeyOperation;
        }

        private static void BuildForeignKeyOperation(
            XDocument edmx, XElement association, ForeignKeyOperation foreignKeyOperation)
        {
            DebugCheck.NotNull(edmx);
            DebugCheck.NotNull(association);
            DebugCheck.NotNull(foreignKeyOperation);

            var principal = association.Descendants(EdmXNames.Ssdl.PrincipalNames).Single();
            var dependent = association.Descendants(EdmXNames.Ssdl.DependentNames).Single();

            var principalRole =
                association.Descendants(EdmXNames.Ssdl.EndNames).Single(
                    r => r.RoleAttribute() == principal.RoleAttribute());
            var dependentRole =
                association.Descendants(EdmXNames.Ssdl.EndNames).Single(
                    r => r.RoleAttribute() == dependent.RoleAttribute());

            var principalTable = GetQualifiedTableNameFromType(edmx, principalRole.TypeAttribute());
            var dependentTable = GetQualifiedTableNameFromType(edmx, dependentRole.TypeAttribute());

            foreignKeyOperation.PrincipalTable = principalTable;
            foreignKeyOperation.DependentTable = dependentTable;

            dependent.Descendants(EdmXNames.Ssdl.PropertyRefNames)
                .Each(pr => foreignKeyOperation.DependentColumns.Add(pr.NameAttribute()));
        }

        private static DbProviderManifest GetProviderManifest(DbProviderInfo providerInfo)
        {
            var providerFactory = DbConfiguration.DependencyResolver.GetService<DbProviderFactory>(providerInfo.ProviderInvariantName);

            return providerFactory.GetProviderServices().GetProviderManifest(providerInfo.ProviderManifestToken);
        }

        private static string GetSchemaQualifiedName(string table, string schema)
        {
            DebugCheck.NotEmpty(table);
            DebugCheck.NotEmpty(schema);

            return new DatabaseName(table, schema).ToString();
        }

        private static string GetQualifiedTableName(XDocument model, string entitySetName)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(entitySetName);

            var schemaAndTable
                = (from es in model.Descendants(EdmXNames.Ssdl.EntitySetNames)
                   where es.NameAttribute().EqualsIgnoreCase(entitySetName)
                   select new
                       {
                           Schema = es.SchemaAttribute(),
                           Table = es.TableAttribute()
                       })
                    .Single();

            return GetSchemaQualifiedName(schemaAndTable.Table, schemaAndTable.Schema);
        }

        private static string GetQualifiedTableNameFromType(XDocument model, string entityTypeName)
        {
            DebugCheck.NotNull(model);
            DebugCheck.NotEmpty(entityTypeName);

            var schemaAndTable
                = (from es in model.Descendants(EdmXNames.Ssdl.EntitySetNames)
                   where es.EntityTypeAttribute().EqualsIgnoreCase(entityTypeName)
                   select new
                       {
                           Schema = es.SchemaAttribute(),
                           Table = es.TableAttribute()
                       })
                    .Single();

            return GetSchemaQualifiedName(schemaAndTable.Table, schemaAndTable.Schema);
        }
    }
}
