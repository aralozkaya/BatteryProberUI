/*
	BatteryProberUI - Implementation of the deprecated Battery Prober project using WPF (C#) with a CLI interface (C++)
    Copyright (C) 2022 Ibrahim Aral Ozkaya

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using System.Runtime.InteropServices;

namespace BatteryProberUI
{
    public static class SystemPowerStatus
    {
        private struct SYSTEM_POWER_STATUS
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte SystemStatusFlag;
            public uint BatteryLifeTime;
            public uint BatteryFullLifeTime;
        }

        [DllImport("Kernel32.dll")]
        private static extern unsafe int GetSystemPowerStatus(SYSTEM_POWER_STATUS* lpSystemPowerStatus);

        static private unsafe SYSTEM_POWER_STATUS Status
        {
            get
            {
                SYSTEM_POWER_STATUS mSystemPowerStatus = new();
                _ = GetSystemPowerStatus(&mSystemPowerStatus);
                return mSystemPowerStatus;
            }
        }
        public static unsafe bool IsAcConnected()
        {
            SYSTEM_POWER_STATUS systemPowerStatus = new();
            _ = GetSystemPowerStatus(&systemPowerStatus);

            return systemPowerStatus.ACLineStatus == 1;
        }
    }
}
