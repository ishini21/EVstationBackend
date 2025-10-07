using EVOwnerManagement.API.Models;

namespace EVOwnerManagement.API.Utils
{
    public static class SlotValidator
    {
        public static bool IsValidPowerCombination(ConnectorType connector, PowerRating power)
        {
            switch (connector)
            {
                case ConnectorType.CHAdeMO_CCS2_DualPort:
                    return power == PowerRating.kW50 || power == PowerRating.kW100;

                case ConnectorType.CCS2_SinglePort:
                    return power == PowerRating.kW30 || power == PowerRating.kW50 || power == PowerRating.kW100;

                case ConnectorType.CHAdeMO_SinglePort:
                    return power == PowerRating.kW30 || power == PowerRating.kW50;

                case ConnectorType.Type2:
                    return power == PowerRating.kW22;

                default:
                    return false;
            }
        }
    }
}
