using System.Collections.Generic;

namespace ElephantSDK
{
    /// <summary>
    /// Implemented by developers for Offer UI actions
    /// </summary>
    public interface IOfferListener
    {
        /// <summary>
        /// Triggered when user interacts with action button
        /// i.e. trying to purchase a product
        /// implement your purchase options on this callback
        /// </summary>
        /// <param name="purchaseOption"> Purchase option for that specific interaction </param>
        void OnLiveOpsOfferPurchaseRequested(PurchaseOption purchaseOption);
        
        /// <summary>
        /// Triggered when UI displayed to user
        /// Implement your pause logics here (if any)
        /// </summary>
        /// <param name="purchaseOptions"> Purchase options for that specific interaction </param>
        void OnLiveOpsOfferShown(List<PurchaseOption> purchaseOptions);
        
        /// <summary>
        /// Triggered when user interacts with close button or outside of the UI
        /// Implement your resume logics here (if any)
        /// </summary>
        /// <param name="purchaseOptions"> Purchase options for that specific interaction </param>
        void OnLiveOpsOfferDismissed(List<PurchaseOption> purchaseOptions);
        
        /// <summary>
        /// Triggered when live ops is ready to show offers
        /// </summary>
        void OnLiveOpsReady();
    }
}