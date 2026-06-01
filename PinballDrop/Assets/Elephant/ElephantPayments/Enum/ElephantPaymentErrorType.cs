namespace ElephantSDK
{
    public enum ElephantPaymentErrorType
    {
        Unknown,
        ProductsFetchFailed,
        StartCheckoutFailed,
        NoEscrowCode,
        PendingCheckFailed,
        ConfirmPurchaseInvalidTransaction,
        ConfirmPurchaseBackendFailed
    }
}