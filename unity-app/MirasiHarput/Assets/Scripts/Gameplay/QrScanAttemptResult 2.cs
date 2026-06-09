public enum QrScanAttemptResult
{
    Success = 0,
    UnknownQr = 1,
    WrongRouteOrder = 2,
    AlreadyTriggered = 3,
    DataNotReady = 4,
    NotQrMode = 5,
    DynamicRedirectQr = 6,
    TriggerRejected = 7
}
