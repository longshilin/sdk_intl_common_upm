#if UNITY_EDITOR && UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_ANDROID
using UnityEditor.Android;
#endif
using UnityEngine;
using XD.Intl.Common.Editor;

public class XDGAndroidPostBuildProcessor : IPostGenerateGradleAndroidProject
{
    public int callbackOrder
    {
        get { return 999; }
    }

    public static void GenerateAndroidX(string path)
    {
        string gradlePropertiesFile = path + "/gradle.properties";

        Debug.Log($"GradleProperties File:{gradlePropertiesFile}");

        if (File.Exists(gradlePropertiesFile))
        {
            File.Delete(gradlePropertiesFile);
        }

        StreamWriter writer = File.CreateText(gradlePropertiesFile);
        writer.WriteLine("org.gradle.jvmargs=-Xmx4096M");
        writer.WriteLine("android.useAndroidX=true");
        writer.WriteLine("android.enableJetifier=true");
        writer.WriteLine("unityStreamingAssets=.unity3d, google-services-desktop.json, google-services.json, GoogleService-Info.plist");
        writer.Flush();
        writer.Close();
    }

    public static bool GeneratedAndroidGradle(string projectPath)
    {
        if (!Directory.Exists(projectPath + "/launcher"))
        {
            Debug.Log($"XDG can't find {projectPath}/launcher");

            string targetGradlePath = projectPath + "/build.gradle";

            string googleServiceOriginPath = projectPath + "/src/main/assets/google-services.json";

            string targetServicePath = projectPath + "/google-services.json";

            if (File.Exists(googleServiceOriginPath))
            {
                //copy google-service.json 到目录下
                Debug.Log($"Copy GoogleService.json to Android Project:{targetServicePath}");
                File.Copy(googleServiceOriginPath, targetServicePath, true);
            }

            if (File.Exists(targetGradlePath))
            {
                XD.Intl.Common.Editor.XDGScriptHandlerProcessor writeHelper =
                    new XD.Intl.Common.Editor.XDGScriptHandlerProcessor(targetGradlePath);
                writeHelper.WriteBelow(@"apply plugin: 'com.android.application'",
                    @"apply plugin: 'com.google.gms.google-services'");
                writeHelper.WriteBelow(@"apply plugin: 'com.android.application'",
                    @"apply plugin: 'com.google.firebase.crashlytics'");
                writeHelper.WriteBelow(@"apply plugin: 'com.android.application'",
                    @"buildscript {dependencies {classpath 'com.google.gms:google-services:4.0.2'}}");
                writeHelper.WriteBelow(@"apply plugin: 'com.android.application'",
                    @"buildscript {dependencies {classpath 'com.google.firebase:firebase-crashlytics-gradle:2.2.1'}}");

                writeHelper.WriteBelow(@"implementation fileTree(dir: 'libs', include: ['*.jar'])", @"

                implementation 'com.google.firebase:firebase-core:18.0.0'
                implementation 'com.google.firebase:firebase-messaging:21.1.0'

                implementation 'com.google.code.gson:gson:2.8.6'
                implementation 'com.google.android.gms:play-services-auth:16.0.1'
                implementation 'com.facebook.android:facebook-login:12.0.0'
                implementation 'com.facebook.android:facebook-share:12.0.0'
                implementation 'com.appsflyer:af-android-sdk:6.4.3'
                implementation 'com.adjust.sdk:adjust-android:4.24.1'
                implementation 'com.android.installreferrer:installreferrer:2.2'
                implementation 'com.android.billingclient:billing:3.0.1'
                implementation 'androidx.recyclerview:recyclerview:1.2.1'
                implementation 'com.google.android.gms:play-services-ads-identifier:15.0.1'
                implementation 'com.twitter.sdk.android:twitter:3.3.0'
                implementation 'com.twitter.sdk.android:tweet-composer:3.3.0'
                implementation 'com.linecorp:linesdk:5.0.1'
                ");
                return true;
            }
            else
            {
                Debug.LogError("TDSGlobal can't find Android Gradle File!");
                return false;
            }
        }

        return false;
    }

    void IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject(string path)
    {
        GenerateAndroidX(path);

        string projectPath = path;

        if (path.Contains("unityLibrary"))
        {
            projectPath = path.Substring(0, path.Length - 12);
            GenerateAndroidX(projectPath);
        }
        else
        {
            GenerateAndroidX(path);
        }

        Debug.Log($"Project path:{path},substring path:{projectPath}");

        if (GeneratedAndroidGradle(projectPath))
        {
            return;
        }

        string googleServiceOriginPath = projectPath + "/unityLibrary/src/main/assets/google-services.json";

        string targetServicePath = projectPath + "/launcher/google-services.json";

        if (File.Exists(googleServiceOriginPath))
        {
            //copy google-service.json 到目录下
            File.Copy(googleServiceOriginPath, targetServicePath, true);
        }

        string launcherGradle = projectPath + "/launcher/build.gradle";

        string baseProjectGradle = projectPath + "/build.gradle";

        string unityLibraryGradle = projectPath + "/unityLibrary/build.gradle";

        if (File.Exists(launcherGradle))
        {
            Debug.Log("write launch gradle");

            XD.Intl.Common.Editor.XDGScriptHandlerProcessor writerHelper =
                new XD.Intl.Common.Editor.XDGScriptHandlerProcessor(launcherGradle);
            writerHelper.WriteBelow(@"implementation project(':unityLibrary')", @"
                
                implementation 'com.google.firebase:firebase-core:18.0.0'
                implementation 'com.google.firebase:firebase-messaging:21.1.0'

            ");
            writerHelper.WriteBelow(@"apply plugin: 'com.android.application'",
                @"apply plugin: 'com.google.gms.google-services'");
            writerHelper.WriteBelow(@"apply plugin: 'com.android.application'",
                @"apply plugin: 'com.google.firebase.crashlytics'");
        }

        if (File.Exists(baseProjectGradle))
        {
            Debug.Log("write project gradle");
            XD.Intl.Common.Editor.XDGScriptHandlerProcessor writerHelper =
                new XD.Intl.Common.Editor.XDGScriptHandlerProcessor(baseProjectGradle);
            writerHelper.WriteBelow(@"task clean(type: Delete) {
    delete rootProject.buildDir
}", @"allprojects {
    buildscript {
        dependencies {
            classpath 'com.google.gms:google-services:4.0.2'
            classpath 'com.google.firebase:firebase-crashlytics-gradle:2.2.1'
        }
    }
}");
        }

        if (File.Exists(unityLibraryGradle))
        {
            XD.Intl.Common.Editor.XDGScriptHandlerProcessor writerHelper =
                new XD.Intl.Common.Editor.XDGScriptHandlerProcessor(unityLibraryGradle);
            writerHelper.WriteBelow(@"implementation fileTree(dir: 'libs', include: ['*.jar'])", @"

                implementation 'com.google.firebase:firebase-core:18.0.0'
                implementation 'com.google.firebase:firebase-messaging:21.1.0'
    
                implementation 'com.google.code.gson:gson:2.8.6'

                implementation 'com.google.android.gms:play-services-auth:16.0.1'
                implementation 'com.facebook.android:facebook-login:12.0.0'
                implementation 'com.facebook.android:facebook-share:12.0.0'
                implementation 'com.appsflyer:af-android-sdk:6.4.3'
                implementation 'com.adjust.sdk:adjust-android:4.24.1'
                implementation 'com.android.installreferrer:installreferrer:2.2'
                implementation 'com.android.billingclient:billing:3.0.0'
                implementation 'androidx.recyclerview:recyclerview:1.2.1'
                implementation 'com.google.android.gms:play-services-ads-identifier:15.0.1'
                implementation 'com.twitter.sdk.android:twitter:3.3.0'
                implementation 'com.twitter.sdk.android:tweet-composer:3.3.0'
                implementation 'com.linecorp:linesdk:5.0.1'
            ");
        }
    }
}
#endif