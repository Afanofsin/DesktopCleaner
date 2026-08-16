using Grid.Services;
using R3;

namespace Grid
{
    public class BinView : IconView
    {
        public void PutInBin(IconView view)
        {
            GameStateService.G.OnIconBinned.OnNext(view);
        }
    }
}