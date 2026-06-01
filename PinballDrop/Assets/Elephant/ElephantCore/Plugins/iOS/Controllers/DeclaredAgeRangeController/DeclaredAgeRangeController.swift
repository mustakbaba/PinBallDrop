import Foundation
import UIKit
import SwiftUI
import DeclaredAgeRange

@objc(DeclaredAgeRangeController)
public class DeclaredAgeRangeController: NSObject {
    enum AgeGates {
        static let childThreshold = 0
        static let teenThreshold = 13
        static let adultThreshold = 18
    }
    
    private enum ResponseStrings {
        static let sharing = "sharing"
        static let declinedSharing = "declinedSharing"
    }
    
    private enum DeclarationStrings {
        static let selfDeclared = "selfDeclared"
        static let paymentChecked = "paymentChecked"
        static let governmentIDChecked = "governmentIDChecked"
        static let checkedByOtherMethod = "checkedByOtherMethod"
        static let guardianDeclared = "guardianDeclared"
        static let guardianPaymentChecked = "guardianPaymentChecked"
        static let guardianGovernmentIDChecked = "guardianGovernmentIDChecked"
        static let guardianCheckedByOtherMethod = "guardianCheckedByOtherMethod"
        static let unknown = "unknown"
    }
    
    private enum ParentalControlStrings {
        static let communicationLimits = "communicationLimits"
        static let significantAppChangeApprovalRequired = "significantAppChangeApprovalRequired"
        static let unknown = "unknown"
    }
    
    private enum JSONKeys {
        static let response = "response"
        static let range = "range"
        static let ageRange = "ageRange"
        static let lowerBound = "lowerBound"
        static let upperBound = "upperBound"
        static let declaration = "ageRangeDeclaration"
        static let parentalControls = "activeParentalControls"
        static let parentalRaw = "parentalControlsRawValue"
    }
    
    @objc public static let sharedInstance = DeclaredAgeRangeController()
    private override init() { super.init() }
    
    public typealias AgeRangeResultBlock = (String?, String?) -> Void
    
    @objc public func requestAgeRange(completion: @escaping AgeRangeResultBlock) {
        guard #available(iOS 26.0, *) else {
            completion(nil, "Declared Age Range API requires iOS 26.0 or later")
            return
        }
        guard let viewController = getRootViewController() else {
            completion(nil, "No root view controller")
            return
        }
        
        Task { @MainActor in
            do {
                let response = try await AgeRangeService.shared.requestAgeRange(
                    ageGates: AgeGates.childThreshold,
                    AgeGates.teenThreshold,
                    AgeGates.adultThreshold,
                    in: viewController
                )
                completion(serializeResponse(response), nil)
            } catch {
                completion(nil, error.localizedDescription)
            }
        }
    }
    
    @objc public func isAvailableSync() -> Bool {
        if #available(iOS 26.0, *) { return true }
        return false
    }
    
    @available(iOS 26.2, *)
    @available(visionOS, unavailable)
    @objc public func isEligibleForAgeFeatures() async -> Bool {
        do {
            return try await AgeRangeService.shared.isEligibleForAgeFeatures
        } catch {
            return false
        }
    }
    
    private func getRootViewController() -> UIViewController? {
        guard var topController = UIApplication.shared
            .connectedScenes
            .compactMap({ $0 as? UIWindowScene })
            .flatMap({ $0.windows })
            .first(where: { $0.isKeyWindow })?
            .rootViewController else { return nil }
        
        while let presented = topController.presentedViewController {
            topController = presented
        }
        return topController
    }
    
    @available(iOS 26.0, *)
    private func parentalControlString(for control: AgeRangeService.ParentalControls) -> String {
        if #available(iOS 26.2, *) {
            if control == .significantAppChangeApprovalRequired {
                return ParentalControlStrings.significantAppChangeApprovalRequired
            }
        }
        
        switch control {
        case .communicationLimits: return ParentalControlStrings.communicationLimits
        default: return ParentalControlStrings.unknown
        }
    }
    
    @available(iOS 26.0, *)
    private func mapDeclaration(_ declaration: AgeRangeService.AgeRangeDeclaration) -> String {
        switch declaration {
        case .selfDeclared: return DeclarationStrings.selfDeclared
        case .paymentChecked: return DeclarationStrings.paymentChecked
        case .governmentIDChecked: return DeclarationStrings.governmentIDChecked
        case .checkedByOtherMethod: return DeclarationStrings.checkedByOtherMethod
        case .guardianDeclared: return DeclarationStrings.guardianDeclared
        case .guardianPaymentChecked: return DeclarationStrings.guardianPaymentChecked
        case .guardianGovernmentIDChecked: return DeclarationStrings.guardianGovernmentIDChecked
        case .guardianCheckedByOtherMethod: return DeclarationStrings.guardianCheckedByOtherMethod
        @unknown default: return DeclarationStrings.unknown
        }
    }
    
    @available(iOS 26.0, *)
    private func serializeResponse(_ response: AgeRangeService.Response) -> String? {
        var json: [String: Any] = [:]
        
        switch response {
        case .sharing(let ageRange):
            json[JSONKeys.response] = ResponseStrings.sharing
            
            var rangeObject: [String: Any] = [:]
            rangeObject[JSONKeys.lowerBound] = ageRange.lowerBound ?? NSNull()
            rangeObject[JSONKeys.upperBound] = ageRange.upperBound ?? NSNull()
            rangeObject[JSONKeys.declaration] = ageRange.ageRangeDeclaration.map { mapDeclaration($0) } ?? NSNull()
            
            var controls: [String] = []
            if ageRange.activeParentalControls.contains(.communicationLimits) {
                controls.append(ParentalControlStrings.communicationLimits)
            }
            if #available(iOS 26.2, *) {
                if ageRange.activeParentalControls.contains(.significantAppChangeApprovalRequired) {
                    controls.append(ParentalControlStrings.significantAppChangeApprovalRequired)
                }
            }
            rangeObject[JSONKeys.parentalControls] = controls
            rangeObject[JSONKeys.parentalRaw] = ageRange.activeParentalControls.rawValue
            
            json[JSONKeys.range] = rangeObject
            
        case .declinedSharing:
            json[JSONKeys.response] = ResponseStrings.declinedSharing
            
        @unknown default:
            json[JSONKeys.response] = "unknown"
        }
        
        return serializeJSON(json)
    }
    
    private func serializeJSON(_ json: [String: Any]) -> String? {
        guard JSONSerialization.isValidJSONObject(json) else { return nil }
        do {
            let data = try JSONSerialization.data(withJSONObject: json)
            return String(decoding: data, as: UTF8.self)
        } catch {
            return nil
        }
    }
}

@available(iOS 26.0, *)
@available(visionOS, unavailable)
@MainActor
public struct DeclaredAgeRangeAction {
    
    public init() {}
    
    public func callAsFunction(ageGates threshold1: Int, _ threshold2: Int? = nil, _ threshold3: Int? = nil) async throws -> AgeRangeService.Response {
        guard let rootVC = UIApplication.shared.connectedScenes
            .compactMap({ $0 as? UIWindowScene })
            .flatMap({ $0.windows })
            .first(where: { $0.isKeyWindow })?
            .rootViewController else {
            throw AgeRangeService.Error.notAvailable
        }
        
        return try await AgeRangeService.shared.requestAgeRange(
            ageGates: DeclaredAgeRangeController.AgeGates.childThreshold,
            DeclaredAgeRangeController.AgeGates.teenThreshold,
            DeclaredAgeRangeController.AgeGates.adultThreshold,
            in: rootVC
        )
    }
}


@available(iOS 26.0, *)
@available(visionOS, unavailable)
@MainActor
extension EnvironmentValues {
    public var requestAgeRange: DeclaredAgeRangeAction {
        get { DeclaredAgeRangeAction() }
        set { }
    }
}
