
using Freedom.Domain.DigestModel.CellViewModels;

namespace Freedom.Domain.Interfaces
{
    public interface IMapPopupAsyncInitializable
    {
        AsyncCellViewModel MapPopup { get; }
    }
}
