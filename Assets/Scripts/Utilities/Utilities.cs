using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public static class Utilities
{

    public static T LoadClass<T>(string filePath)
    {
        string fileName = filePath;
        if (File.Exists(fileName))
        {
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (FileStream fileStr = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    return (T)bf.Deserialize(fileStr);
                }
            } catch
            {
                return default(T);
            }
        }
        return default(T);
    }
	
    public static void SaveClass<T>(string filePath, T saveClass)
    {
        if (saveClass != null)
        {
            string fileName = filePath;
            BinaryFormatter bf = new BinaryFormatter();
            Directory.CreateDirectory(Application.persistentDataPath + @"/BubbleSurvivor");
            using (FileStream fileStr = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                bf.Serialize(fileStr, saveClass);
            }
        }
    }

    public static BannerView RequestBanner(string _adUnitID, AdSize adSize, AdPosition adPos)
    {
        #if UNITY_ANDROID
        //string adUnitId = "INSERT_ANDROID_BANNER_AD_UNIT_ID_HERE";
        string adUnitId = _adUnitID;
        #elif UNITY_IPHONE
        //string adUnitId = "INSERT_IOS_BANNER_AD_UNIT_ID_HERE";
        string adUnitId = _adUnitID;
        #else
        //string adUnitId = "unexpected_platform";
        string adUnitId = _adUnitID;
        #endif
        
        // Create a 320x50 banner at the top of the screen.
        BannerView bannerView = new BannerView(adUnitId, adSize, adPos);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the banner with the request.
        bannerView.LoadAd(request);
        return bannerView;
    }

}
