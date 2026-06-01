using System;
using UnityEngine;
using UnityEngine.UI;

namespace ElephantSDK
{
    /// <summary>
    /// Loads popup game images from Resources/ElephantUI/GameImages.
    /// Place sprites named {PopupType}GameImage (e.g. GdprGameImage) in any
    /// Resources/ElephantUI/GameImages folder to customize popup backgrounds.
    /// </summary>
    public static class PopupGameImageLoader
    {
        private const string ImagesResourcePath = "ElephantUI/GameImages";

        public static Sprite LoadGameImageSprite(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return null;
            }

            var prefixLower = prefix.Trim().ToLowerInvariant();
            var gameImagePrefixLower = (prefixLower + "gameimage");

            var sprites = Resources.LoadAll<Sprite>(ImagesResourcePath);
            if (sprites != null)
            {
                foreach (var sprite in sprites)
                {
                    if (sprite != null && sprite.name.Trim().ToLowerInvariant().StartsWith(gameImagePrefixLower))
                    {
                        return sprite;
                    }
                }
            }

            var textures = Resources.LoadAll<Texture2D>(ImagesResourcePath);
            if (textures != null)
            {
                foreach (var tex in textures)
                {
                    if (tex != null && tex.name.Trim().ToLowerInvariant().StartsWith(gameImagePrefixLower))
                    {
                        return TextureToSprite(tex);
                    }
                }
            }

            return null;
        }

        public static Sprite LoadGameImageSpriteForPopup(PopupType popupType)
        {
            string prefix = popupType switch
            {
                PopupType.Tos => "Tos",
                PopupType.Vppa => "Vppa",
                PopupType.ForceUpdate => "ForceUpdate",
                PopupType.Blocked => "Blocked",
                PopupType.Ccpa => "Ccpa",
                PopupType.Gdpr => "Gdpr",
                PopupType.Pin => "Pin",
                PopupType.Loading => "Loading",
                PopupType.Error => "Error",
                PopupType.Settings => "Settings",
                PopupType.NetworkOffline => "NetworkOffline",
                PopupType.InGameSettings => "InGameSettings",
                PopupType.Social => "Social",
                PopupType.AgeBlocked => "AgeBlocked",
                PopupType.Collectibles => "Collectibles",
                PopupType.Alert => "Alert",
                _ => popupType.ToString()
            };
            return LoadGameImageSprite(prefix);
        }

        public static void SetupGameImage(PopupType popupType, Image gameImage)
        {
            if (gameImage == null)
            {
                return;
            }    

            var sprite = LoadGameImageSpriteForPopup(popupType);
            if (sprite != null)
            {
                gameImage.sprite = sprite;
                gameImage.gameObject.SetActive(true);
				gameImage.preserveAspect = true;
            }
            else
            {
                gameImage.sprite = null;
                gameImage.gameObject.SetActive(false);
            }
        }

        private static Sprite TextureToSprite(Texture2D texture)
        {
            if (texture == null) 
            {
                return null;
            }
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}
