using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.CrossCutting.Identity
{
    public class RateLimitPolices
    {
        public const string AdminLogin = "AdminLoginPolicy";
        public const string PatientLogin = "PatientLoginPolicy";
        public const string AppointmentBooking = "AppointmentBookingPolicy";
        public const string General = "GeneralPolicy";

    }
}
