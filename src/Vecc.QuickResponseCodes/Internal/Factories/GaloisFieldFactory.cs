using Vecc.QuickResponseCodes.Internal.Utils;

namespace Vecc.QuickResponseCodes.Internal.Factories
{
    public class GaloisFieldFactory : IGaloisFieldFactory
    {
        private readonly IGaloisField _defaultGaloisField;

        public GaloisFieldFactory()
        {
            this._defaultGaloisField = new GaloisField(0x11D);
        }

        public IGaloisField Create(uint polynomial)
        {
            //use the cached one if possible
            if (polynomial == 0x11D)
            {
                return this._defaultGaloisField;
            }

            return new GaloisField(polynomial);
        }
    }
}
