using System;

namespace Hermer29.Almasury
{
    public class MetadataBuilder
    {
        private readonly ContractPrefs _prefs;
        
        public MetadataBuilder(ContractPrefs prefs)
        {
            _prefs = prefs;
        }

        public void AsFirstState()
        {
            if (_prefs.instance is State == false)
            {
                throw new InvalidOperationException("Trying to make not state first state");
            }
            _prefs.isFirstState = true;
        }
    }
}