using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace ElephantSDK
{
    [Serializable]
    public class Compliance
    {
        public ComplianceTosResponse tos;
        public ComplianceCcpaResponse ccpa;
        public ComplianceCcpaResponse gdpr_ad_consent;
        public ComplianceBlockedResponse blocked;
        public ComplianceTosResponse vppa;

        public Compliance()
        {
            tos = new ComplianceTosResponse();
            ccpa = new ComplianceCcpaResponse();
            gdpr_ad_consent = new ComplianceCcpaResponse();
            blocked = new ComplianceBlockedResponse();
            vppa = new ComplianceTosResponse();
        }
    }
    
    [Serializable]
    public class PlayerData
    {
        public string app_id;
        public string player_id;
        public string zis_json;
    }

    [Serializable]
    public class GdprOptionData
    {
        public string action;
        public string data;
    }
    
    [Serializable]
    public class AgeGateData
    {
        public bool required;
        public string data;
    }

    [Serializable]
    public class AgeGate
    {
        public AgeGateData age_input;
        public AgeGateData age_fail;

        public AgeGate()
        {
            age_input = new AgeGateData();
            age_fail = new AgeGateData();
        }
    }
    
    [Serializable]
    public class OpenResponse
    {
        public string user_id;
        public string campaign_name;
        public string segment_name;
        public string player_id;
        public PlayerData player_data;
        public bool consent_required;
        public bool consent_status;
        public long remote_config_version;
        public string remote_config_json;
        public string gdpr_body_text;
        public string gdpr_option_1;
        public string gdpr_option_2;
        public string gdpr_option_3;
        public GdprOptionData gdpr_option_1_data;
        public GdprOptionData gdpr_option_2_data;
        public GdprOptionData gdpr_option_3_data;
        public string user_country;
        public string country;
        public AdConfig ad_config;
        public InternalConfig internal_config;
        public List<MirrorData> mirror_data;
        public Compliance compliance;
        public string hash;
        public long server_time;
        public AgeGate age_gate;
        public SegmentConfig segment_config;

        public OpenResponse()
        {
            this.user_id = "";
            this.campaign_name = "";
            this.segment_name = "";
            this.player_id = "";
            this.consent_required = false;
            this.remote_config_json = JsonConvert.SerializeObject(new ConfigResponse());
            this.ad_config = new AdConfig();
            this.internal_config = new InternalConfig();
            this.mirror_data = new List<MirrorData>();
            this.compliance = new Compliance();
            this.hash = "";
            remote_config_json = "";
            remote_config_version = 0;
            player_data = new PlayerData();
            gdpr_option_1_data = new GdprOptionData();
            gdpr_option_2_data = new GdprOptionData();
            gdpr_option_3_data = new GdprOptionData();
            age_gate = new AgeGate();
            segment_config = new SegmentConfig();
        }
    }
}