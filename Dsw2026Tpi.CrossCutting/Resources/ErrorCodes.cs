namespace Dsw2026Tpi.CrossCutting.Resources;

public static class ErrorCodes
{
    public const string UNHANDLED_ERROR = "UNHANDLED_ERROR";
    public const string AUTHENTICATION_FAILED = "AUTHENTICATION_FAILED";
    public const string AUTHORIZATION_FAILED = "AUTHORIZATION_FAILED";
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string ENTITY_NOTFOUND = "ENTITY_NOTFOUND";
    public const string REGISTER_USER_INVALID = "REGISTER_USER_INVALID";
    public const string REGISTER_USER_CONFLICT = "REGISTER_USER_CONFLICT";
    public const string SPECIALITY_DUPLICATED = "SPECIALITY_DUPLICATED";
    public const string SLOT_ALREADY_BOOKED = "SLOT_ALREADY_BOOKED";
    public const string APPOINTMENT_NOT_FOUND = "APPOINTMENT_NOT_FOUND";
    public const string APPOINTMENT_ALREADY_CANCELLED = "APPOINTMENT_ALREADY_CANCELLED";
    public const string APPOINTMENT_IN_PAST = "APPOINTMENT_IN_PAST";
    public const string DOCTOR_NOT_AVAILABLE = "DOCTOR_NOT_AVAILABLE";
    public const string INVALID_DAY = "INVALID_DAY";
    public const string TIME_RANGE_INVALID = "TIME_RANGE_INVALID";
    public const string OVERLAPPING_SCHEDULE = "OVERLAPPING_SCHEDULE";
    public const string PATIENT_NOT_FOUND = "PATIENT_NOT_FOUND";
    public const string DOCTOR_ALREADY_HAS_AVAILABILITY = "DOCTOR_ALREADY_HAS_AVAILABILITY";
}