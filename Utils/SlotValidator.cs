/*
 * Course: SE4040 - Enterprise Application Development
 * Assignment: EV Station Management System - Station Management
 * Student: IT22071248 - PEIRIS M S M
 * Created On : October 7, 2025
 * File Name: SlotValidator.cs
 * 
 * This file contains the SlotValidator utility class, which provides validation logic
 * for checking valid power and connector type combinations for charging slots.
 */
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
