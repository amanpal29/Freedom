using System;
using DemoDataBuilder.Importer;
using GemBox.Spreadsheet;
using log4net;

namespace DemoDataBuilder
{
    public class Program
    {
        static void Main(string[] args)
        {
            SpreadsheetInfo.SetLicense("EKME-U49D-6EOV-UCRK");

            ThreadContext.Stacks["FileName"].Push(string.Empty);
            ThreadContext.Stacks["Position"].Push(string.Empty);

            Engine engine = new Engine();

            engine.ImportCore(@"..\..\..\..\SampleData\Core\BusinessTypes.xml");
            engine.ImportCore(@"..\..\..\..\SampleData\Core\ContactTypes.xml");
            engine.ImportCore(@"..\..\..\..\SampleData\Core\FacilityTypes.xml");
            engine.ImportCore(@"..\..\..\..\SampleData\Core\OperationsTypes.xml");

            engine.AddAreaCode(403, 12d);
            engine.AddAreaCode(587, 3d);

            engine.AddAreaCode(800, 3d);
            engine.AddAreaCode(866, 1d);
            engine.AddAreaCode(877, 1d);
            engine.AddAreaCode(888, 1d);

            engine.Add(new SheetImporter(@"T:\Sample Data\Corrected June 2013 Training Sample Database March 2013.xlsx", "All Merged"));

            engine.Execute();

            Console.Out.WriteLine("Press <Enter> to close...");
            Console.In.ReadLine();
        }
    }
}

