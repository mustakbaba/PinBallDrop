using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace ElephantSDK
{
    public static class ElephantPayments
    {
        #region Public API

        public static Action<List<ElephantProduct>> OnProductsFetched;
        public static Action<ElephantPayment> OnPaymentPending;
        public static Action<ElephantPaymentsError> OnError;
        
        public static Action OnPaymentDialogueClosed;
        
        private static bool _isPurchaseFlowActive;
        
        private static Coroutine _activePollingCoroutine;
        
        private static bool _isTestMode;

        public static void Initialize(
            Action<List<ElephantProduct>> onProductsFetched,
            Action<ElephantPayment> onPaymentPending,
            Action<ElephantPaymentsError> onError)
        {
            if (_isInitialized)
                return;
            
            _isTestMode = RemoteConfig.GetInstance().GetBool("ds_payments_test_enabled", false);
            ElephantConstants.IsDevUrlEnabled = RemoteConfig.GetInstance().GetBool("ds_payments_dev_env_enabled", false);

            OnProductsFetched = onProductsFetched;
            OnPaymentPending = onPaymentPending;
            OnError = onError;

            _notifiedPendingThisSession.Clear();
            LoadConfirmationQueue();

#if !UNITY_EDITOR
            Elephant.OnWebViewClosed += OnWebViewClosed;
            Elephant.OnApplicationFocusTrue += OnAppFocus;
#endif

            _isInitialized = true;

            FetchProducts();
            CheckPendingPayments();
            ProcessConfirmationQueue();
        }

        public static void FetchProducts()
        {
            if (ElephantCore.Instance == null)
            {
                RaiseError(
                    ElephantPaymentErrorType.ProductsFetchFailed,
                    "ElephantCore.Instance is null while fetching products."
                );
                return;
            }

            Elephant.Event(EVENT_PRODUCT_FETCHING, -1);

            var data = ListPaymentsRequest.Create();
            var json = JsonConvert.SerializeObject(data);
            var bodyJson = JsonConvert.SerializeObject(
                new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID())
            );

            var networkManager = new GenericNetworkManager<List<ElephantProduct>>();
            var coroutine = networkManager.PostWithResponse(
                ElephantConstants.DS_LIST_PRODUCTS,
                bodyJson,
                response =>
                {
                    if (response?.data == null)
                        response.data = new List<ElephantProduct>();

                    var productsFetchedParams = Params.New()
                        .Set("product_count", response.data.Count);
                    Elephant.Event(EVENT_PRODUCT_FETCHED, -1, productsFetchedParams);

                    OnProductsFetched?.Invoke(response.data);
                },
                error =>
                {
                    RaiseError(
                        ElephantPaymentErrorType.ProductsFetchFailed,
                        "Failed to fetch products.",
                        error
                    );
                }
            );

            ElephantCore.Instance.StartCoroutine(coroutine);
        }

        public static void PurchaseWeb(string productId, Action onPaymentDialogueClosed = null)
        {
            var eventParams = Params.New().Set("product_id", productId);
            Elephant.Event(EVENT_ESCROW_STARTED, -1, eventParams);

            if (!_isInitialized)
            {
                RaiseError(
                    ElephantPaymentErrorType.StartCheckoutFailed,
                    "ElephantPayments not initialized.",
                    productId: productId
                );
                return;
            }

            if (ElephantCore.Instance == null)
            {
                RaiseError(
                    ElephantPaymentErrorType.StartCheckoutFailed,
                    "ElephantCore.Instance is null.",
                    productId: productId
                );
                return;
            }
            
            OnPaymentDialogueClosed = onPaymentDialogueClosed;


#if !UNITY_EDITOR
            bool useWebView = RemoteConfig.GetInstance().GetBool("ds_webview_enabled", true);
#else
            bool useWebView = false;
#endif

            var data = StartCheckoutRequest.Create(productId, _isTestMode);
            var json = JsonConvert.SerializeObject(data);
            var bodyJson = JsonConvert.SerializeObject(
                new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID())
            );

            var networkManager = new GenericNetworkManager<PaymentCheckoutResponse>();
            var coroutine = networkManager.PostWithResponse(
                ElephantConstants.DS_START_CHECKOUT,
                bodyJson,
                response =>
                {
                    var escrowCode = response?.data?.escrowCode;
                    var expiry = response?.data != null ? response.data.expiry : 0L;

                    if (string.IsNullOrEmpty(escrowCode))
                    {
                        RaiseError(
                            ElephantPaymentErrorType.NoEscrowCode,
                            "Checkout could not start: no escrow code returned.",
                            productId: productId
                        );

                        var escrowFailedParams = Params.New().Set("product_id", productId);
                        Elephant.Event(EVENT_ESCROW_FAILED, -1, escrowFailedParams);
                        return;
                    }

                    var escrowCompletedParams = Params.New()
                        .Set("product_id", productId)
                        .Set("escrow_code", escrowCode)
                        .Set("expiry", expiry);
                    Elephant.Event(EVENT_ESCROW_COMPLETED, -1, escrowCompletedParams);

                    var url = BuildCheckoutUrl(escrowCode, expiry, useWebView);

                    var checkoutStartedParams = Params.New()
                        .Set("url", url);
                    
                    _isPurchaseFlowActive = true;
                    _hasLoggedCheckingEvent = false;

#if UNITY_IOS && !UNITY_EDITOR
                    Elephant.Event(EVENT_CHECKOUT_STARTED, -1, checkoutStartedParams);
                    if (useWebView)
                        ElephantIOS.openURLInWebView(url);
                    else
                        ElephantIOS.openURL(url);
#elif UNITY_ANDROID && !UNITY_EDITOR
                    bool useChromeCustomTabs = RemoteConfig.GetInstance().GetBool("ds_use_chrome_custom_tabs", false);
                    
                    if (useChromeCustomTabs)
                    {
                        url = BuildCheckoutUrl(escrowCode, expiry, false);
                        checkoutStartedParams = Params.New().Set("url", url).Set("method", "chrome_custom_tabs");
                    }
                    
                    Elephant.Event(EVENT_CHECKOUT_STARTED, -1, checkoutStartedParams);
                    
                    if (useChromeCustomTabs)
                        ElephantAndroid.openURLInChromeCustomTab(url);
                    else if (useWebView)
                        ElephantAndroid.openURLInWebView(url);
                    else
                        Application.OpenURL(url);
#else
                    Elephant.Event(EVENT_CHECKOUT_STARTED, -1, checkoutStartedParams);
                    Application.OpenURL(url);
#endif
                },
                error =>
                {
                    RaiseError(
                        ElephantPaymentErrorType.StartCheckoutFailed,
                        "Purchase failed while starting checkout.",
                        error,
                        productId
                    );

                    var escrowFailedParams = Params.New().Set("product_id", productId);
                    Elephant.Event(EVENT_ESCROW_FAILED, -1, escrowFailedParams);
                }
            );

            ElephantCore.Instance.StartCoroutine(coroutine);
        }
        
        public static void CheckForPendingPurchases()
        {
            CheckPendingPayments();
        }


        public static void ConfirmPurchase(ElephantPayment payment)
        {
            if (payment == null || string.IsNullOrEmpty(payment.transactionId))
            {
                RaiseError(
                    ElephantPaymentErrorType.ConfirmPurchaseInvalidTransaction,
                    "ConfirmPurchase called with null or empty transactionId."
                );
                return;
            }

            var txId = payment.transactionId;

            var known =
                _knownPendingTransactionIds.Contains(txId) ||
                _confirmationQueue.Contains(txId);

            if (!known)
            {
                RaiseError(
                    ElephantPaymentErrorType.ConfirmPurchaseInvalidTransaction,
                    "ConfirmPurchase called for transaction that is not reported as pending.",
                    productId: payment.purchasedProduct != null ? payment.purchasedProduct.productId : null,
                    transactionId: txId
                );
                return;
            }

            if (!_confirmationQueue.Contains(txId))
            {
                var price = payment.purchasedProduct?.price ?? 0;

                ElephantCore.Instance?.AdjustElephantAdapter?.TrackPurchaseRevenue(
                    AdjustTokens.Ds_payment,
                    price,
                    "USD"
                );

                _confirmationQueue.Add(txId);
                _pendingPaymentInfo[txId] = payment;
                SaveConfirmationQueue();
            }

            ProcessConfirmationQueue();
        }

        #endregion

        #region Internal state

        private static bool _isInitialized;
        private static bool _isCheckingPurchases;
        private static bool _hasLoggedCheckingEvent;

        private static HashSet<string> _confirmationQueue = new();
        private static HashSet<string> _activeConfirmations = new();
        private static HashSet<string> _knownPendingTransactionIds = new();
        private static Dictionary<string, ElephantPayment> _pendingPaymentInfo = new();

        private static HashSet<string> _notifiedPendingThisSession = new();

        private const string PlayerPrefsQueueKey = "ELEPHANT_CONFIRM_QUEUE";

        private const string EVENT_ESCROW_STARTED = "elephant_ds_escrow_started";
        private const string EVENT_ESCROW_COMPLETED = "elephant_ds_escrow_completed";
        private const string EVENT_ESCROW_FAILED = "elephant_ds_escrow_failed";
        private const string EVENT_CHECKOUT_STARTED = "elephant_ds_checkout_started";
        private const string EVENT_CHECKOUT_COMPLETED = "elephant_ds_checkout_completed";
        private const string EVENT_PAYMENT_CHECKING = "elephant_ds_payment_checking";
        private const string EVENT_PAYMENT_PENDING = "elephant_ds_payment_pending";
        private const string EVENT_PAYMENT_COMPLETED = "elephant_ds_payment_completed";
        private const string EVENT_PRODUCT_FETCHING = "elephant_ds_product_fetching";
        private const string EVENT_PRODUCT_FETCHED = "elephant_ds_product_fetched";

        #endregion

        #region Lifecycle callbacks

        private static void OnAppFocus()
        {
            if (_isPurchaseFlowActive)
            {
                _isPurchaseFlowActive = false;
                OnPaymentDialogueClosed?.Invoke();
                
                var checkoutCompletedParams = Params.New()
                    .Set("reason", "focus");
                Elephant.Event(EVENT_CHECKOUT_COMPLETED, -1, checkoutCompletedParams);

                StartPollingIfNeeded();
            }
            else
            {
                CheckPendingPayments();
            }

            ProcessConfirmationQueue();
        }
        
        private static void OnWebViewClosed(string reason)
        {
            if (_isPurchaseFlowActive)
            {
                _isPurchaseFlowActive = false;
                OnPaymentDialogueClosed?.Invoke();
                
                var checkoutCompletedParams = Params.New()
                    .Set("reason", reason);
                Elephant.Event(EVENT_CHECKOUT_COMPLETED, -1, checkoutCompletedParams);

                StartPollingIfNeeded();
            }
            else
            {
                CheckPendingPayments();
            }
    
            ProcessConfirmationQueue();
        }
        
        private static void StartPollingIfNeeded()
        {
            if (ElephantCore.Instance == null)
                return;

            if (_activePollingCoroutine != null)
            {
                ElephantCore.Instance.StopCoroutine(_activePollingCoroutine);
                _activePollingCoroutine = null;
            }

            _activePollingCoroutine = ElephantCore.Instance.StartCoroutine(PollForPendingPayments());
        }
        
        private static IEnumerator PollForPendingPayments()
        {
            var maxAttempts = RemoteConfig.GetInstance().GetInt("ds_polling_max_attempts", 8);
            var initialInterval = RemoteConfig.GetInstance().GetFloat("ds_polling_initial_interval", 1f);
            var maxInterval = RemoteConfig.GetInstance().GetFloat("ds_polling_max_interval", 32f);
            var timeout = RemoteConfig.GetInstance().GetFloat("ds_polling_timeout", 5f);

            var initialCount = _notifiedPendingThisSession.Count;
            var currentInterval = initialInterval;

            for (int i = 0; i < maxAttempts; i++)
            {
                var isComplete = false;
                CheckPendingPayments(timeout, () => isComplete = true);

                while (!isComplete)
                {
                    yield return null;
                }

                if (_notifiedPendingThisSession.Count > initialCount)
                {
                    _activePollingCoroutine = null;
                    yield break;
                }

                yield return new WaitForSeconds(currentInterval);

                currentInterval = Mathf.Min(currentInterval * 2f, maxInterval);
            }

            _activePollingCoroutine = null;
        }


        #endregion

        #region Pending payments

        private static void CheckPendingPayments(float? timeout = null, Action onComplete = null)
        {
            if (_isCheckingPurchases)
            {
                onComplete?.Invoke();
                return;
            }

            if (!_isInitialized || ElephantCore.Instance == null)
            {
                onComplete?.Invoke();
                return;
            }

            if (!_hasLoggedCheckingEvent)
            {
                _hasLoggedCheckingEvent = true;
                Elephant.Event(EVENT_PAYMENT_CHECKING, -1);
            }

            _isCheckingPurchases = true;

            var data = ListPaymentsRequest.Create();
            var json = JsonConvert.SerializeObject(data);
            var bodyJson = JsonConvert.SerializeObject(
                new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID())
            );
            var networkManager = new GenericNetworkManager<List<ElephantPayment>>();
            var coroutine = networkManager.PostWithResponse(
                ElephantConstants.DS_LIST_PAYMENTS,
                bodyJson,
                response =>
                {
                    _isCheckingPurchases = false;
                    OnPendingPurchasesReceived(response);
                    onComplete?.Invoke();
                },
                error =>
                {
                    _isCheckingPurchases = false;
                    RaiseError(
                        ElephantPaymentErrorType.PendingCheckFailed,
                        "Failed to check pending payments.",
                        error
                    );
                    onComplete?.Invoke();
                },
                timeout
            );

            ElephantCore.Instance.StartCoroutine(coroutine);
        }

        private static void OnPendingPurchasesReceived(GenericResponse<List<ElephantPayment>> response)
        {
            _knownPendingTransactionIds.Clear();

            if (response.data == null || response.data.Count == 0)
                return;

            foreach (var pending in response.data)
            {
                if (pending == null || string.IsNullOrEmpty(pending.transactionId))
                    continue;

                _knownPendingTransactionIds.Add(pending.transactionId);

                if (_confirmationQueue.Contains(pending.transactionId))
                    continue;

                if (_notifiedPendingThisSession.Contains(pending.transactionId))
                    continue;

                var purchase = new ElephantPayment
                {
                    purchasedProduct = pending.purchasedProduct,
                    transactionId = pending.transactionId
                };

                var productId = purchase.purchasedProduct?.productId;
                var price = purchase.purchasedProduct?.price ?? 0f;

                var paymentPendingParams = Params.New()
                    .Set("product_id", productId)
                    .Set("price", price)
                    .Set("transaction_id", purchase.transactionId);
                Elephant.Event(EVENT_PAYMENT_PENDING, -1, paymentPendingParams);

                if (OnPaymentPending != null)
                {
                    _notifiedPendingThisSession.Add(pending.transactionId);
                    OnPaymentPending.Invoke(purchase);
                }
            }
        }

        #endregion

        #region Confirmation queue

        private static void ProcessConfirmationQueue()
        {
            if (_confirmationQueue.Count == 0)
                return;

            if (ElephantCore.Instance == null)
                return;

            var idsToProcess = _confirmationQueue
                .Where(id => !_activeConfirmations.Contains(id))
                .ToList();

            if (idsToProcess.Count == 0)
                return;

            foreach (var transactionId in idsToProcess)
            {
                _activeConfirmations.Add(transactionId);

                ElephantCore.Instance.StartCoroutine(
                    NotifyPurchaseClaimed(
                        transactionId,
                        () =>
                        {
                            _pendingPaymentInfo.TryGetValue(transactionId, out var payment);
                            var productId = payment?.purchasedProduct?.productId;
                            var price = payment?.purchasedProduct?.price ?? 0f;

                            var paymentCompletedParams = Params.New()
                                .Set("product_id", productId)
                                .Set("price", price)
                                .Set("transaction_id", transactionId);
                            Elephant.Event(EVENT_PAYMENT_COMPLETED, -1, paymentCompletedParams);

                            _confirmationQueue.Remove(transactionId);
                            _activeConfirmations.Remove(transactionId);
                            _pendingPaymentInfo.Remove(transactionId);
                            SaveConfirmationQueue();
                        },
                        error =>
                        {
                            _activeConfirmations.Remove(transactionId);

                            RaiseError(
                                ElephantPaymentErrorType.ConfirmPurchaseBackendFailed,
                                "Failed to confirm purchase with backend.",
                                error,
                                transactionId: transactionId
                            );

                            SaveConfirmationQueue();
                        }
                    )
                );
            }
        }

        private static IEnumerator NotifyPurchaseClaimed(
            string transactionId,
            Action onSuccess,
            Action<string> onError)
        {
            var data = MarkPaymentProcessedRequest.Create(transactionId);
            var json = JsonConvert.SerializeObject(data);
            var bodyJson = JsonConvert.SerializeObject(
                new ElephantData(json, ElephantCore.Instance.GetCurrentSession().GetSessionID())
            );

            var networkManager = new GenericNetworkManager<object>();
            return networkManager.PostWithResponse(
                ElephantConstants.DS_MARK_PAYMENT_PROCESSED,
                bodyJson,
                _ => onSuccess?.Invoke(),
                error => onError?.Invoke(error)
            );
        }

        #endregion

        #region Persistence

        private static void SaveConfirmationQueue()
        {
            try
            {
                var list = new List<string>(_confirmationQueue);
                var queueJson = JsonConvert.SerializeObject(list);
                PlayerPrefs.SetString(PlayerPrefsQueueKey, queueJson);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                RaiseError(
                    ElephantPaymentErrorType.Unknown,
                    "Failed to save confirmation queue.",
                    e.Message
                );
            }
        }

        private static void LoadConfirmationQueue()
        {
            _confirmationQueue.Clear();

            if (!PlayerPrefs.HasKey(PlayerPrefsQueueKey))
                return;

            try
            {
                var json = PlayerPrefs.GetString(PlayerPrefsQueueKey);
                var list = JsonConvert.DeserializeObject<List<string>>(json);
                if (list != null)
                    _confirmationQueue = new HashSet<string>(list);
            }
            catch (Exception e)
            {
                RaiseError(
                    ElephantPaymentErrorType.Unknown,
                    "Failed to load confirmation queue, clearing local data.",
                    e.Message
                );
                _confirmationQueue.Clear();
            }
        }

        #endregion

        #region Helpers

        private static string BuildCheckoutUrl(string escrowCode, long expiry, bool useWebView)
        {
            var checkoutVar = RemoteConfig.GetInstance().Get("ds_checkout_var", "default");
            var isCheckoutDevUrl = RemoteConfig.GetInstance().GetBool("ds_payments_checkout_dev_url_enabled", false);
            
            var baseUrl = isCheckoutDevUrl 
                ? "https://www-develop.payments.rollic.store/checkout-v3?" 
                : "https://payments.rollic.store/checkout-v3?";

            var builder = new System.Text.StringBuilder();
            builder.Append(baseUrl);
            builder.Append("code=").Append(escrowCode);
            builder.Append("&expiry=").Append(expiry);
            builder.Append("&checkout_context=").Append(useWebView ? "zeroshot_webview" : "zeroshot_direct");
            builder.Append("&checkout_var=").Append(checkoutVar);

#if UNITY_IOS
            builder.Append("&client_id=2");
#elif UNITY_ANDROID
            builder.Append("&client_id=3");
#endif

            if (useWebView)
            {
                builder.Append("&auto_redirect=true");
                builder.Append("&success_path=/checkout-completed-successfully");
            }
            else
            {
                var customSuccessPath = RemoteConfig.GetInstance().Get("ds_checkout_success_path", "");
                if (!string.IsNullOrEmpty(customSuccessPath))
                {
                    builder.Append("&success_path=").Append(customSuccessPath);
                }
            }

            return builder.ToString();
        }

        private static void RaiseError(
            ElephantPaymentErrorType type,
            string message,
            string rawError = null,
            string productId = null,
            string transactionId = null)
        {
            var log = "[ElephantPayments] " + message;
            if (!string.IsNullOrEmpty(rawError))
                log += " | " + rawError;

            ElephantLog.LogError("ElephantPayments", log);

            OnError?.Invoke(
                new ElephantPaymentsError
                {
                    Type = type,
                    Message = message,
                    RawError = rawError,
                    ProductId = productId,
                    TransactionId = transactionId
                }
            );
        }

        #endregion
    }
}