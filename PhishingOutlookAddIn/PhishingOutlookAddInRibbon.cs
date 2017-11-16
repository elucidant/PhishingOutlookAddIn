﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Tools.Ribbon;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace PhishingOutlookAddIn
{
   public partial class PhishingOutlookAddInRibbon
   {
      private static readonly log4net.ILog log =
         log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      private static bool menuItemsLoaded = false;

      public const string APPLICATION_NAME = "PhishingOutlookAddIn";
      public const string APPLICATION_REGISTRY_NAME = "PhishingAddIn";

      public const string DEFAULT_PHISHING_EMAIL_ADDRESS =
         "phishing@yourbusiness.com";
      public const string DEFAULT_PHISHING_EMAIL_SUBJECT =
         "POTENTIAL PHISHING";
      public const string DEFAULT_PHISHING_EMAIL_FOLDER = "Deleted Items";
      public const string DEFAULT_ORGANIZATION_NAME = "IT";
      public const string DEFAULT_PHISHING_INFORMATION_URL =
         "https://en.wikipedia.org/wiki/Phishing";
      public const string DEFAULT_ABOUT_INFO =
         "The Phishing Outlook Add-In will provide an end-user{0}" +
         "the ability to report Phishing eMails to the IT group,{0}" +
         "sending the Phish email as an attachment to the Phish{0}" +
         "Reporting eMail.The Phish eMail will be moved or{0}" +
         "deleted depending on how the Add-In is configured.{0}{0}" +
         "For additional help with the plugin, please contact your{0}" +
         "IT support team.";

      public const int DEFAULT_PHISHING_EMAIL_MAX_REPORTED = 10;

      public const bool DEFAULT_SHOW_SETTINGS = false;
      public const bool DEFAULT_PHISHING_EMAIL_ADDIN_DEBUG = false;
      public const bool DEFAULT_PHISHING_EMAIL_DELETE_COMPLETE = false;
      public const bool DEFAULT_PHISHING_EMAIL_CONFIRMATION_PROMPT = false;

      public const string DELETED_ITEMS_FOLDER_NAME = "Deleted Items";

      // Add-In Registry constants...
      public const string ADD_IN_REGISTRY_ROOT = "HKEY_LOCAL_MACHINE";
      public const string ADD_IN_REGISTRY_OFFICE_15_RESILIENCY_PATH =
         "Software\\Microsoft\\Office\\15.0\\Outlook\\Resiliency\\DoNotDisableAddinList";
      public const string ADD_IN_REGISTRY_OFFICE_16_RESILIENCY_PATH =
         "Software\\Microsoft\\Office\\16.0\\Outlook\\Resiliency\\DoNotDisableAddinList";
      public const string ADD_IN_REGISTRY_ADDIN_PATH =
         "Software\\Microsoft\\Office\\Outlook\\Addins\\PhishingAddIn";
      public const string ADD_IN_REGISTRY_ADDIN_DEFAULTS_PATH =
         ADD_IN_REGISTRY_ADDIN_PATH + "\\Defaults";

      // Add-In Registry Keys
      public const string ADD_IN_REGISTRY_SHOW_SETTINGS_KEY =
         "ShowSettings";
      public const string ADD_IN_REGISTRY_ADDRESS_KEY =
         "PhishingEmailAddress";
      public const string ADD_IN_REGISTRY_SUBJECT_KEY =
         "PhishingEmailSubject";
      public const string ADD_IN_REGISTRY_FOLDER_KEY =
         "PhishingEmailFolder";
      public const string ADD_IN_REGISTRY_MAX_REPORTED_KEY =
         "PhishingEmailMaxReported";
      public const string ADD_IN_REGISTRY_DELETE_COMPLETE_KEY =
         "PhishingEmailDeleteComplete";
      public const string ADD_IN_REGISTRY_ORGANIZATION_KEY =
         "OrganizationName";
      public const string ADD_IN_REGISTRY_DEBUG_KEY =
         "AddInDebug";
      public const string ADD_IN_REGISTRY_PHISHING_INFORMATION_URL_KEY =
         "PhishingInformationURL";
      public const string ADD_IN_REGISTRY_ABOUT_INFO_KEY =
         "AboutInfo";
      public const string ADD_IN_REGISTRY_CONFIRMATION_PROMPT_KEY =
         "PhishingEmailConfirmationPrompt";

      public const string INITIALIZED_FILE_NAME = "user.ini";

      /**
       * 
       *  Add-In Property DEFAULT values are initially set to the application
       *  default values from source code control.  The application will need
       *  to override these settings based on what is in REGISTRY.
       *  
       */
      public static string DEFAULT_PHISHING_EMAIL_ADDRESS_PROPERTY =
         DEFAULT_PHISHING_EMAIL_ADDRESS;
      public static string DEFAULT_PHISHING_EMAIL_SUBJECT_PROPERTY =
         DEFAULT_PHISHING_EMAIL_SUBJECT;
      public static string DEFAULT_PHISHING_EMAIL_FOLDER_PROPERTY =
         DEFAULT_PHISHING_EMAIL_FOLDER;
      public static string DEFAULT_ORGANIZATION_NAME_PROPERTY =
         DEFAULT_ORGANIZATION_NAME;
      public static string DEFAULT_PHISHING_INFORMATION_URL_PROPERTY =
         DEFAULT_PHISHING_INFORMATION_URL;
      public static string DEFAULT_ABOUT_INFO_PROPERTY = DEFAULT_ABOUT_INFO;

      public static int DEFAULT_PHISHING_EMAIL_MAX_REPORTED_PROPERTY =
         DEFAULT_PHISHING_EMAIL_MAX_REPORTED;

      public static bool SHOW_SETTINGS_PROPERTY = DEFAULT_SHOW_SETTINGS;
      public static bool DEFAULT_PHISHING_EMAIL_DELETE_COMPLETE_PROPERTY =
         DEFAULT_PHISHING_EMAIL_DELETE_COMPLETE;
      public static bool DEFAULT_ADDIN_DEBUG_PROPERTY =
         DEFAULT_PHISHING_EMAIL_ADDIN_DEBUG;
      public static bool DEFAULT_PHISHING_EMAIL_CONFIRMATION_PROMPT_PROPERTY =
         DEFAULT_PHISHING_EMAIL_CONFIRMATION_PROMPT;
      public enum KeyValueTypes
      {
         String = 1,
         Integer = 2,
         Boolean = 3
      }

      private PhishingOutlookAddInSettingsForm form = null;
      private PhishingOutlookAddInAboutForm form2 = null;
      private PhishingOutlookAddInEmailConfirmationForm sendConfirmationForm = null;

      /**
       * 
       * Static constructor to be called upon class initialization and before
       * instance creation.  This constructor cannot be called directly.  The
       * static constructor will get the Registry key values that will be used
       * by the Add-In.
       *
       */

      static PhishingOutlookAddInRibbon()
      {
         log4net.Config.XmlConfigurator.Configure();

         log.Debug("In PhishingOutlookAddInRiboon Constructor...");

         // Set Defaults to Registry values...the AddIn will always get the
         // defaults from the registry in the case the user would want to
         // reset their settings back to the default values.
         setDefaultsToRegistryKeyValues();

         // Upon initialization of the object, we need to determine if this is
         // the first run of the application.  We need to check for the
         // existence of the Initialized file in the User's local roaming
         // folder for the AddIn.  If it does not exist, we need to load
         // the properties from the Registry.

         if (isAddInInitialized() == true)
         {
            log.Debug(
               "Initialized flag file DOES EXIST - initialization file creation skipped!");

            log.Debug("Reading user property settings...");

            Dictionary<string, string> userProperties = readInitializedFile();

            log.Debug("Setting user property settings...");

            setUserPropertySettings(userProperties);
         }
         else
         {
            log.Debug(
               "Initialized flag does not EXIST.  Initializing application " +
               "settings from the registry.  Creating initialized file...");
            
            // We don't need to set the properties at this point since they
            // are defaulted to the Registry Key values.  Just need to
            // persist the Properties to the file system and then create the
            // initialized file...
            persistPropertySettings();

            // Create the Initialized file as a flag for future runs that we
            // don't initialize these values again.
            createInitializedFile();
         }

         logSettings();
      }

      /**
       * 
       * This method will attempt to create the Registry Keys that are required
       * by the application.  Currently, this is not used by the application
       * but leaving it here for historical reasons.
       * 
       * Note:  One of the Registry Keys for the AddIn is the Resiliency Key.
       * This registry value would force Outlook to always load the Phishing
       * AddIn. Outlook will disable an add-in that it believes causes Outlook
       * to crash, but if the Resiliency Key Value is defined, Outlook won’t
       * disable an add-in because it loaded too slow.
       * 
       * For the setting of the Resiliency Key for PhishningAddIn, there
       * are two solutions to address when Outlook DISABLES the AddIn upon
       * Outlook startup:
       * 
       * 1. End User can go to File/Manage COM Add-ins on Outlook 2016 and click
       * the "Always enable this add-in" button to set the
       * "Phishing Outlook Add-In" configured on the HKEY_CURRENT_USER
       * Resiliency Registry key value:
       * 
       * HKEY_CURRENT_USER\Software\Microsoft\Office\16.0\Outlook\Resiliency\
       * DoNotDisableAddinList\PhishingAddIn = 0x00000001 (REG_DWORD)
       * 
       * 2. Another option is to define a GPO that sets the Resiliency Registry
       * Key value.
       * 
       */

      public static void createRegistryKeys()
      {
         log.Debug("Attempting to create Resiliency Registry Key Values...");

         // Attempt to set AddIn Resiliency Registry Key values if NOT set.

         // Need to set for both Office v15.0 and v16.0
         // ADD_IN_REGISTRY_OFFICE_15_RESILIENCY_PATH

         createHCURegistryKeyDWordValue(
            ADD_IN_REGISTRY_OFFICE_15_RESILIENCY_PATH,
            APPLICATION_REGISTRY_NAME,
            1,
            false);

         // ADD_IN_REGISTRY_OFFICE_16_RESILIENCY_PATH
         createHCURegistryKeyDWordValue(
            ADD_IN_REGISTRY_OFFICE_16_RESILIENCY_PATH,
            APPLICATION_REGISTRY_NAME,
            1,
            false);
      }

      /**
       * 
       * This method checks if the AddIn has been initialized based on the
       * presence of an initialized.txt file in the User's application data
       * folder.  Was having issues with being able to set the Initialized
       * registry flag, so going to use a "folder" flag instead.
       * 
       */
      public static bool isAddInInitialized()
      {
         bool isInitialized = false;

         string localAppData =
            Environment.GetFolderPath(
               System.Environment.SpecialFolder.ApplicationData);

         string addInLocalAppData =
            Path.Combine(localAppData, APPLICATION_NAME);

         string initializedFilePath =
            Path.Combine(addInLocalAppData, INITIALIZED_FILE_NAME);

         if (File.Exists(initializedFilePath) == true)
         {
            isInitialized = true;
         }

         return isInitialized;
      }

      /**
       * 
       * Creates initialized file on the file system.  Used as a flag to
       * indicate that the AddIn has been initialized upon first installation.
       * Initialization is executed via the use of Registry Default keys for
       * the AddIn.
       * 
       */
      public static void createInitializedFile()
      {
         string localAppData =
            Environment.GetFolderPath(
               System.Environment.SpecialFolder.ApplicationData);

         string addInLocalAppData =
            Path.Combine(localAppData, APPLICATION_NAME);

         try
         {
            // Need to check if the directory exists...if not, create all
            // directories and subdirectories in the path unless they
            // already exist...
            if (Directory.Exists(addInLocalAppData) == false)
            {
               Directory.CreateDirectory(addInLocalAppData);
            }

            string initializedFilePath =
               Path.Combine(addInLocalAppData, INITIALIZED_FILE_NAME);

            log.Debug("Creating the Initialized User settings file: " +
               initializedFilePath);

            string initFileContent = buildUserSettingsFileContent();

            System.IO.StreamWriter file =
               new System.IO.StreamWriter(initializedFilePath);

            file.WriteLine(initFileContent);

            file.Close();
         }
         catch (System.Exception ex)
         {
            log.Error("Exception found during createInitializedFile(): " +
               ex.Message);
         }
      }

      /**
       * 
       * Deletes initialized file on the file system.
       * 
       */
      public static void deleteInitializedFile()
      {
         string localAppData =
            Environment.GetFolderPath(
               System.Environment.SpecialFolder.ApplicationData);

         string addInLocalAppData =
            Path.Combine(localAppData, APPLICATION_NAME);

         try
         {
            // Need to check if the directory exists...if not, no need to
            // delete.
            if (Directory.Exists(addInLocalAppData) == true)
            {
               string initializedFilePath =
                  Path.Combine(addInLocalAppData, INITIALIZED_FILE_NAME);

               log.Debug("Deleting the Initialized User settings file: " +
                  initializedFilePath);

               File.Delete(initializedFilePath);
            }
         }
         catch (System.Exception ex)
         {
            log.Error("Exception found during deleteInitializedFile(): " +
               ex.Message);
         }
      }
      
      /**
       * 
       * Updates initialized file on the file system. 
       * 
       */
      public static void updateInitializedFile()
      {
         log.Debug("Updating the Initialized User settings file...");

         // Will need to first delete the current initialized file...
         deleteInitializedFile();

         // Now recreate initialized file...
         createInitializedFile();
      }

      /**
       * 
       * Reads User Initialization file.
       * 
       */

      public static Dictionary<String, String> readInitializedFile()
      {
         Dictionary<String, String> userProperties = new Dictionary<String, String>();

         string localAppData =
            Environment.GetFolderPath(
               System.Environment.SpecialFolder.ApplicationData);

         string addInLocalAppData =
            Path.Combine(localAppData, APPLICATION_NAME);

         try
         {
            // Need to check if the directory exists...if not, no need to
            // delete.
            if (Directory.Exists(addInLocalAppData) == true)
            {
               string initializedFilePath =
                  Path.Combine(addInLocalAppData, INITIALIZED_FILE_NAME);

               using (StreamReader sr = new StreamReader(initializedFilePath))
               {
                  string line;

                  while ((line = sr.ReadLine()) != null)
                  {
                     if (line != "")
                     {
                        // Trim out whitespace characters before and after...
                        line.Trim();

                        if ((line[0] != '#') || (line[0] != '['))
                        {
                           string[] parts = line.Split(new char[] { '=' });

                           if (parts.Length == 2)
                           {
                              userProperties[parts[0]] = parts[1];
                           }
                        }
                     }
                  }
               }
            }
         }
         catch (System.Exception ex)
         {
            log.Error("Exception found during readInitializedFile(): " +
               ex.Message);
         }

         foreach (KeyValuePair<string, string> kvp in userProperties)
         {
            log.Debug("Key = " + kvp.Key + "; Value = " + kvp.Value);
         }

         return userProperties;
      }

      /**
       * 
       * This method persists the current application settings to the Add-In
       * User property file on the user's file system.  This is how the
       * settings are used across Outlook application life-cycles for the
       * specific user.  This will called ONLY when an initialization file
       * has not been generated for the user on thes system.
       * 
       */

      public static void persistPropertySettings()
      {
         // Now we need to persist the values to the Properties file...
         Properties.Settings.Default.phishingEmailFolder =
            DEFAULT_PHISHING_EMAIL_FOLDER_PROPERTY;
         Properties.Settings.Default.phishingEmailAddress =
            DEFAULT_PHISHING_EMAIL_ADDRESS_PROPERTY;
         Properties.Settings.Default.phishingEmailSubject =
            DEFAULT_PHISHING_EMAIL_SUBJECT_PROPERTY;
         Properties.Settings.Default.organizationName =
            DEFAULT_ORGANIZATION_NAME_PROPERTY;
         Properties.Settings.Default.phishingEmailMaxReported =
            DEFAULT_PHISHING_EMAIL_MAX_REPORTED_PROPERTY;
         Properties.Settings.Default.phishingEmailDeleteComplete =
            DEFAULT_PHISHING_EMAIL_DELETE_COMPLETE_PROPERTY;
         Properties.Settings.Default.phishingInformationURL =
            DEFAULT_PHISHING_INFORMATION_URL_PROPERTY;
         Properties.Settings.Default.phishingEmailConfirmationPrompt =
            DEFAULT_PHISHING_EMAIL_CONFIRMATION_PROMPT_PROPERTY;
         Properties.Settings.Default.aboutInfo =
            DEFAULT_ABOUT_INFO_PROPERTY;

         Properties.Settings.Default.addinDebug = DEFAULT_ADDIN_DEBUG_PROPERTY;
         Properties.Settings.Default.showSettings = SHOW_SETTINGS_PROPERTY;

         // Persist changes to user settings between application sessions.
         Properties.Settings.Default.Save();
      }

      public static void setUserPropertySettings(Dictionary<string,string> userProperties)
      {
         // Now we need to persist the values to the Properties file...
         Properties.Settings.Default.phishingEmailFolder =
            userProperties["phishingEmailFolder"];
         Properties.Settings.Default.phishingEmailAddress =
            userProperties["phishingEmailAddress"];
         Properties.Settings.Default.phishingEmailSubject =
            userProperties["phishingEmailSubject"];
         Properties.Settings.Default.organizationName =
            userProperties["organizationName"];
         Properties.Settings.Default.phishingInformationURL =
            userProperties["phishingInformationURL"];
         Properties.Settings.Default.aboutInfo =
            userProperties["aboutInfo"];

         // Process integer values...
         int integerValue = 0;

         if (int.TryParse(userProperties["phishingEmailMaxReported"], out integerValue))
         {
            Properties.Settings.Default.phishingEmailMaxReported = integerValue;
         }
         else
         {
            // If the configuration file does not provide a valid number, we
            // will just use the default value from the Registry.
            Properties.Settings.Default.phishingEmailMaxReported =
               DEFAULT_PHISHING_EMAIL_MAX_REPORTED_PROPERTY;
         }

         // Process boolean values...
         bool flag;

         if (Boolean.TryParse(userProperties["addInDebug"], out flag))
         {
            Properties.Settings.Default.addinDebug = flag;
         }
         else
         {
            Properties.Settings.Default.addinDebug =
               DEFAULT_ADDIN_DEBUG_PROPERTY;
         }

         if (Boolean.TryParse(userProperties["phishingEmailDeleteComplete"], out flag))
         {
            Properties.Settings.Default.phishingEmailDeleteComplete = flag;
         }
         else
         {
            Properties.Settings.Default.phishingEmailDeleteComplete =
               DEFAULT_PHISHING_EMAIL_DELETE_COMPLETE_PROPERTY;
         }

         if (Boolean.TryParse(userProperties["phishingEmailConfirmationPrompt"], out flag))
         {
            Properties.Settings.Default.phishingEmailConfirmationPrompt = flag;
         }
         else
         {
            Properties.Settings.Default.phishingEmailConfirmationPrompt =
               DEFAULT_PHISHING_EMAIL_CONFIRMATION_PROMPT_PROPERTY;
         }

         if (Boolean.TryParse(userProperties["showSettings"], out flag))
         {
            Properties.Settings.Default.showSettings = flag;
         }
         else
         {
            Properties.Settings.Default.showSettings =
               SHOW_SETTINGS_PROPERTY;
         }
      }

      /**
       * 
       * This method retrieves ALL the Add-In's needed properties from the
       * Add-In's Properties object.
       * 
       */
      public static void setDefaultsToPropertyValues()
      {
         log.Debug("Setting AddIn Defaults to User Property values...");

         DEFAULT_PHISHING_EMAIL_FOLDER_PROPERTY =
            Properties.Settings.Default.phishingEmailFolder;
         
         DEFAULT_PHISHING_EMAIL_ADDRESS_PROPERTY =
            Properties.Settings.Default.phishingEmailAddress;

         DEFAULT_PHISHING_EMAIL_SUBJECT_PROPERTY =
            Properties.Settings.Default.phishingEmailSubject;

         DEFAULT_ORGANIZATION_NAME_PROPERTY =
            Properties.Settings.Default.organizationName;

         DEFAULT_PHISHING_EMAIL_MAX_REPORTED_PROPERTY =
            Properties.Settings.Default.phishingEmailMaxReported;

         DEFAULT_PHISHING_EMAIL_DELETE_COMPLETE_PROPERTY =
            Properties.Settings.Default.phishingEmailDeleteComplete;

         DEFAULT_PHISHING_INFORMATION_URL_PROPERTY =
            Properties.Settings.Default.phishingInformationURL;

         DEFAULT_PHISHING_EMAIL_CONFIRMATION_PROMPT_PROPERTY =
            Properties.Settings.Default.phishingEmailConfirmationPrompt;

         DEFAULT_ABOUT_INFO_PROPERTY = Properties.Settings.Default.aboutInfo;

         DEFAULT_ADDIN_DEBUG_PROPERTY = Properties.Settings.Default.addinDebug;
         SHOW_SETTINGS_PROPERTY = Properties.Settings.Default.showSettings;
      }

      public static void logSettings()
      {
         log.Debug(
            "Main AddIn Properties:\n" + buildAddInSettingsString());

         log.Debug(
            "AddIn User Configuration Properties:\n" +
            buildUserConfigSettingsString());
      }

      public static string buildUserSettingsFileContent()
      {
         string userSettingsFileString =
            "###\n" +
            "### This is the Phishing AddIn User Configuration file!\n" +
            "###\n\n" +
            "[User Settings]\n" +
            "phishingEmailFolder=" +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_EMAIL_FOLDER_PROPERTY + "\n" +
            "phishingEmailAddress=" +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_EMAIL_ADDRESS_PROPERTY + "\n" +
            "phishingEmailSubject=" +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_EMAIL_SUBJECT_PROPERTY + "\n" +
            "organizationName=" +
               PhishingOutlookAddInRibbon.DEFAULT_ORGANIZATION_NAME_PROPERTY + "\n" +
            "phishingEmailMaxReported=" +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_EMAIL_MAX_REPORTED_PROPERTY + "\n" +
            "phishingEmailDeleteComplete=" +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_EMAIL_DELETE_COMPLETE_PROPERTY + "\n" +
            "phishingInformationURL=" +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_INFORMATION_URL_PROPERTY + "\n" +
            "phishingEmailConfirmationPrompt=" +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_EMAIL_CONFIRMATION_PROMPT_PROPERTY + "\n" +
            "aboutInfo=" +
               Properties.Settings.Default.aboutInfo + "\n" +
            "addInDebug=" +
               PhishingOutlookAddInRibbon.DEFAULT_ADDIN_DEBUG_PROPERTY + "\n" +
            "showSettings=" +
               PhishingOutlookAddInRibbon.SHOW_SETTINGS_PROPERTY;

         return userSettingsFileString;
      }

      public static string buildAddInSettingsString()
      {
         string addInSettingsString =
            "Phishing Email Folder: " +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_EMAIL_FOLDER_PROPERTY + "\n" +
            "Phishing Email Address: " +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_EMAIL_ADDRESS_PROPERTY + "\n" +
            "Phishing Email Subject: " +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_EMAIL_SUBJECT_PROPERTY + "\n" +
            "Organization Name: " +
               PhishingOutlookAddInRibbon.DEFAULT_ORGANIZATION_NAME_PROPERTY + "\n" +
            "Phishing Email Max Reported: " +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_EMAIL_MAX_REPORTED_PROPERTY + "\n" +
            "Phishing Email Delete Complete: " +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_EMAIL_DELETE_COMPLETE_PROPERTY + "\n" +
            "Phishing Information URL: " +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_INFORMATION_URL_PROPERTY + "\n" +
            "Phishing Email Confirmation Prompt: " +
               PhishingOutlookAddInRibbon.DEFAULT_PHISHING_EMAIL_CONFIRMATION_PROMPT_PROPERTY + "\n" +
            "About Info: " +
               Properties.Settings.Default.aboutInfo + "\n" +
            "AddIn Debug: " +
               PhishingOutlookAddInRibbon.DEFAULT_ADDIN_DEBUG_PROPERTY + "\n" +
            "Show Settings: " +
               PhishingOutlookAddInRibbon.SHOW_SETTINGS_PROPERTY;

         return addInSettingsString;
      }

      /**
       * 
       * Helper method to build User Properties string, typically used for
       * DEBUG purposes.
       * 
       */
      public static string buildUserConfigSettingsString()
      {
         string userConfigSettingsString =
            "Phishing Email Folder: " +
               Properties.Settings.Default.phishingEmailFolder + "\n" +
            "Phishing Email Address: " +
               Properties.Settings.Default.phishingEmailAddress + "\n" +
            "Phishing Email Subject: " +
               Properties.Settings.Default.phishingEmailSubject + "\n" +
            "Organization Name: " +
               Properties.Settings.Default.organizationName + "\n" +
            "Phishing Email Maximum Reported: " +
               Properties.Settings.Default.phishingEmailMaxReported + "\n" +
            "Phishing Email Delete Complete: " +
               Properties.Settings.Default.phishingEmailDeleteComplete + "\n" +
            "Phishing Information URL: " +
               Properties.Settings.Default.phishingInformationURL + "\n" +
            "Phishing Email Confirmation Prompt: " +
               Properties.Settings.Default.phishingEmailConfirmationPrompt + "\n" +
            "About Info: " +
               Properties.Settings.Default.aboutInfo + "\n" +
            "Debug Enabled: " + Properties.Settings.Default.addinDebug + "\n" +
            "Show Settings: " + Properties.Settings.Default.showSettings;

         return userConfigSettingsString;
      }

      /**
       * 
       * This method retrieves ALL the Add-In's needed properties from the
       * Windows Registry.
       * 
       */
      private static void setDefaultsToRegistryKeyValues()
      {
         object returnValue = null;

         // Get Registry setting for ShowSettings key.
         returnValue = retrieveHLMRegistryKeyValue(
            ADD_IN_REGISTRY_ADDIN_PATH,
            ADD_IN_REGISTRY_SHOW_SETTINGS_KEY,
            KeyValueTypes.Boolean);

         if (returnValue != null)
         {
            SHOW_SETTINGS_PROPERTY = (bool)returnValue;
         }

         returnValue = retrieveHLMRegistryKeyValue(
            ADD_IN_REGISTRY_ADDIN_DEFAULTS_PATH,
            ADD_IN_REGISTRY_ADDRESS_KEY,
            KeyValueTypes.String);

         if (returnValue != null)
         {
            DEFAULT_PHISHING_EMAIL_ADDRESS_PROPERTY = (string)returnValue;
         }

         returnValue = retrieveHLMRegistryKeyValue(
            ADD_IN_REGISTRY_ADDIN_DEFAULTS_PATH,
            ADD_IN_REGISTRY_SUBJECT_KEY,
            KeyValueTypes.String);

         if (returnValue != null)
         {
            DEFAULT_PHISHING_EMAIL_SUBJECT_PROPERTY = (string)returnValue;
         }

         returnValue = retrieveHLMRegistryKeyValue(
            ADD_IN_REGISTRY_ADDIN_DEFAULTS_PATH,
            ADD_IN_REGISTRY_FOLDER_KEY,
            KeyValueTypes.String);

         if (returnValue != null)
         {
            DEFAULT_PHISHING_EMAIL_FOLDER_PROPERTY = (string)returnValue;
         }

         returnValue = retrieveHLMRegistryKeyValue(
            ADD_IN_REGISTRY_ADDIN_DEFAULTS_PATH,
            ADD_IN_REGISTRY_ORGANIZATION_KEY,
            KeyValueTypes.String);

         if (returnValue != null)
         {
            DEFAULT_ORGANIZATION_NAME_PROPERTY = (string)returnValue;
         }

         returnValue = retrieveHLMRegistryKeyValue(
            ADD_IN_REGISTRY_ADDIN_DEFAULTS_PATH,
            ADD_IN_REGISTRY_MAX_REPORTED_KEY,
            KeyValueTypes.Integer);

         if (returnValue != null)
         {
            DEFAULT_PHISHING_EMAIL_MAX_REPORTED_PROPERTY = (int)returnValue;
         }

         returnValue = retrieveHLMRegistryKeyValue(
            ADD_IN_REGISTRY_ADDIN_DEFAULTS_PATH,
            ADD_IN_REGISTRY_DELETE_COMPLETE_KEY,
            KeyValueTypes.Boolean);

         if (returnValue != null)
         {
            DEFAULT_PHISHING_EMAIL_DELETE_COMPLETE_PROPERTY = (bool)returnValue;
         }

         returnValue = retrieveHLMRegistryKeyValue(
            ADD_IN_REGISTRY_ADDIN_DEFAULTS_PATH,
            ADD_IN_REGISTRY_DEBUG_KEY,
            KeyValueTypes.Boolean);

         if (returnValue != null)
         {
            DEFAULT_ADDIN_DEBUG_PROPERTY = (bool)returnValue;
         }

         returnValue = retrieveHLMRegistryKeyValue(
            ADD_IN_REGISTRY_ADDIN_DEFAULTS_PATH,
            ADD_IN_REGISTRY_PHISHING_INFORMATION_URL_KEY,
            KeyValueTypes.String);

         if (returnValue != null)
         {
            DEFAULT_PHISHING_INFORMATION_URL_PROPERTY = (string)returnValue;
         }

         returnValue = retrieveHLMRegistryKeyValue(
            ADD_IN_REGISTRY_ADDIN_DEFAULTS_PATH,
            ADD_IN_REGISTRY_ABOUT_INFO_KEY,
            KeyValueTypes.String);

         if (returnValue != null)
         {
            DEFAULT_ABOUT_INFO_PROPERTY = (string)returnValue;
         }

         returnValue = retrieveHLMRegistryKeyValue(
            ADD_IN_REGISTRY_ADDIN_DEFAULTS_PATH,
            ADD_IN_REGISTRY_CONFIRMATION_PROMPT_KEY,
            KeyValueTypes.Boolean);

         if (returnValue != null)
         {
            DEFAULT_PHISHING_EMAIL_CONFIRMATION_PROMPT_PROPERTY = (bool)returnValue;
         }
      }

      private void Ribbon1_Load(object sender, RibbonUIEventArgs e)
      {
         try
         {
            // Initialize the forms that will be used with the ribbon...
            form = new PhishingOutlookAddInSettingsForm();
            form2 = new PhishingOutlookAddInAboutForm();
            sendConfirmationForm = new PhishingOutlookAddInEmailConfirmationForm();

            this.group1.Label = String.Format(
               this.group1.Label, DEFAULT_ORGANIZATION_NAME_PROPERTY);
         }
         catch (System.Exception ex)
         {
            MessageBox.Show(ex.Message);
         }
      }

      private void menu1_ItemsLoading(object sender, Microsoft.Office.Tools.Ribbon.RibbonControlEventArgs e)
      {
         // Had to add the following condition because everytime the Menu was accessed,
         // the ItemsLoading event kept getting fired and the would keep adding the same
         // menu items over and over again!!!!
         if (menuItemsLoaded == false)
         {
            if (Properties.Settings.Default.showSettings == true)
            {
               RibbonButton menuButton1 = Factory.CreateRibbonButton();

               menuButton1.Label = "&Phish Reporting Settings";
               menuButton1.Click +=
                  new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(
                     phishReportingSettingsForm_Click);

               menu1.Items.Add(menuButton1);
            }

            RibbonButton menuButton2 = Factory.CreateRibbonButton();

            menuButton2.Label = "Phishing Information";
            menuButton2.Click +=
               new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(
                  phishingOutlookAddInPhishingInformation_Click);

            menu1.Items.Add(menuButton2);

            RibbonButton menuButton3 = Factory.CreateRibbonButton();

            menuButton3.Label = "About";
            menuButton3.Click +=
               new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(
                  phishingOutlookAddInAboutForm_Click);

            menu1.Items.Add(menuButton3);

            menuItemsLoaded = true;
         }
      }

      private void phishReportingSettingsForm_Click(object sender, RibbonControlEventArgs e)
      {
         // Need to force the update here...in the case where the show
         // confirmation dialog setting is updated outside the settings form.
         PhishingOutlookAddInSettingsForm.updateUserSettings();

         // Display the form to the user...
         form.showForm();
      }

      private void phishingOutlookAddInPhishingInformation_Click(object sender, RibbonControlEventArgs e)
      {
         Process.Start(@DEFAULT_PHISHING_INFORMATION_URL_PROPERTY);
      }

      private void phishingOutlookAddInAboutForm_Click(object sender, RibbonControlEventArgs e)
      {
         // Display the form to the user...
         form2.Show();
      }

      private void button1_Click(object sender, RibbonControlEventArgs e)
      {
         // Process to receive active selection and save email files only
         Microsoft.Office.Interop.Outlook.Explorer currExplorer = null;
         Microsoft.Office.Interop.Outlook.MailItem currMail = null;

         log.Debug("User selected button to Report a Phish!");

         if (Properties.Settings.Default.phishingEmailConfirmationPrompt == true)
         {
            var confirmResult = sendConfirmationForm.ShowDialog();

            // If the user indicates NO OR clicked the close button on the
            // modal dialog, then we just exit out...
            if ((confirmResult == DialogResult.No) ||
               (confirmResult == DialogResult.Cancel))
            {
               log.Debug("User Cancelled Confirmation or opted to NOT send the Phishing Email!");

               return;
            } 
         }

         try
         {
            currExplorer = Globals.ThisAddIn.Application.ActiveExplorer();

            // Always look for the mail in the Explorer.  Previously, if the
            // Inspector was NOT NULL, we used the inspector to see process
            // the selecteed item.   However, due to customer feedback, we
            // should only process what is selected in the Explorer.
            if (currExplorer != null)
            {                  
               if (log.IsDebugEnabled == true)
               {
                  log.Debug("Found explorer...checking selected emails...");

                  Microsoft.Office.Interop.Outlook.MAPIFolder selectedFolder =
                     currExplorer.CurrentFolder;

                  String expMessage =
                     "Your current folder is " +
                     selectedFolder.Name + ".\n";

                  log.Debug(expMessage);
               }

               // Verify that the MAXIMUM or less emails have been
               // selected...
               if (currExplorer.Selection.Count > form.PhishingEmailMaxReported)
               {
                  MessageBox.Show(
                     "Please select " +
                     form.PhishingEmailMaxReported +
                     " or LESS emails to report!",
                     "Maximum Reported Phishing Emails Exceeded!",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Error);

                  return;
               }
               else if (currExplorer.Selection.Count == 0)
               {
                  MessageBox.Show(
                     "Please select an eMail to report as a Phish!");

                  return;
               }
               else
               {
                  // We know that there is at least as many as the MAXIMUM
                  // NUMBER of emails selected at this point...
                  Object selObject = null;

                  for (int i = 0; i < currExplorer.Selection.Count; i++)
                  {
                     // Selection array is ONE based for some reason...
                     selObject = currExplorer.Selection[i + 1];

                     if (selObject is Microsoft.Office.Interop.Outlook.MailItem)
                     {
                        currMail = (selObject as Microsoft.Office.Interop.Outlook.MailItem);
                     }
                     else
                     {
                        // At this point, if the select object is not a MailItem object
                        // we just want to ignore the rest of the operation and return.
                        string objectType = selObject.GetType().ToString();

                        string message =
                           "The object selected is NOT a Mail Item.  " +
                           "This will NOT be processed as a Phish!";

                        MessageBox.Show(message,
                           "Warning",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Warning);

                        log.Warn(message);

                        return;
                     }

                     processMail(currMail);
                  }
               }
            }
         }
         catch (System.Exception ex)
         {
            MessageBox.Show(ex.Message);
         }
      }

      /**
       * 
       * This method processes a selected email.
       * 
       */

      private void processMail(Microsoft.Office.Interop.Outlook.MailItem currMail)
      {
         if (currMail == null)
         {
            MessageBox.Show(
               "No EMAIL has been selected for processing!",
               "Information",
               MessageBoxButtons.OK,
               MessageBoxIcon.Information);
         }
         else
         {
            log.Debug(
               "Processing Email - Sender: " +
               currMail.SenderName + "; Subject: " +
               currMail.Subject);

            // Forward the email...
            forwardMailAsAttachement(currMail);

            log.Debug(
               "Forwarded eMail was Sent!  " +
               "Moving email to the Outlook " +
               "folder: " +
               form.PhishingEmailFolder);

            // Add check to see if we are deleting the eMail instead of moving
            // it to a specified eMail folder...
            if (form.PhishingEmailDeleteComplete == true)
            {
               permanentlyDeleteEmail(currMail);
            }
            else
            {
               moveEmail(currMail, form.PhishingEmailFolder);
            }
         }
      }

      /**
       * 
       * This method will permanently delete an email from Outlook, similar to
       * the use of the shift+delete manually on an email in Outlook.  
       * Surprise, surprise...Microsoft does not have a method to easily
       * perform a PERMANENT DELETE of an email...you literally have to move
       * the phish eMail to the Deleted Items folder first, and then delete it
       * from there.  This is the only way, at least since the writing of this
       * comment, that we can permanently delete an email out of Outlook in
       * Visual C#
       * 
       */
      private void permanentlyDeleteEmail(
         Microsoft.Office.Interop.Outlook.MailItem currMail)
      {
         Microsoft.Office.Interop.Outlook.Explorer currExplorer = 
            Globals.ThisAddIn.Application.ActiveExplorer();

         Microsoft.Office.Interop.Outlook.Store store =
            currExplorer.CurrentFolder.Store;

         Microsoft.Office.Interop.Outlook.MAPIFolder deletedItemsFolder =
            store.GetRootFolder().Folders[DELETED_ITEMS_FOLDER_NAME];

         // The move here will retain a reference to the MailItem entity that
         // is moved to the Deleted items folder...this is good because we
         // don't need to search for this email in Deleted Items...thank the
         // Maker!
         Microsoft.Office.Interop.Outlook.MailItem movedMail =
            currMail.Move(deletedItemsFolder);

         // Stupid Microsoft action here...need to change a value to trigger a
         // Save...otherwise, upcoming Delete will NOT OCCUR!!!!  
         movedMail.Subject = movedMail.Subject + " ";

         // Need to save it...
         movedMail.Save();

         // Now, permanently delete it!
         movedMail.Delete();
      }

      /**
       * 
       * Moves a specified email to a specified destination folder by name.
       * 
       */

      private Microsoft.Office.Interop.Outlook.MailItem moveEmail(
         Microsoft.Office.Interop.Outlook.MailItem currMail,
         string destinationFolderName)
      {
         Microsoft.Office.Interop.Outlook.Explorer currExplorer =
            Globals.ThisAddIn.Application.ActiveExplorer();

         Microsoft.Office.Interop.Outlook.Store store =
            currExplorer.CurrentFolder.Store;

         // Move the current email to User's selected Mail Box...
         Microsoft.Office.Interop.Outlook.MAPIFolder destFolder =
            store.GetRootFolder().Folders[destinationFolderName];

         return currMail.Move(destFolder);
      }

      /**
       * 
       * Need to forward email as an attachment.  Why?  Forwarding as an
       * attachment is a way to share the email body in exactly the form
       * you received it. This makes it easier for the recipient of your
       * forward to reply to the original sender and it preserves some
       * message details that can otherwise be lost â€” useful to help
       * troubleshooting email problems, for example.
       * 
       */

      private void forwardMailAsAttachement(Microsoft.Office.Interop.Outlook.MailItem mail)
      {
         if (mail == null)
         {
            MessageBox.Show("No EMAIL to forward!");
         }
         else
         {
            string forwardRecipient = form.PhishingEmailAddress;
            string forwardSubject = form.PhishingEmailSubject;

            // Create newMail object from original mail object's Forward() call...
            Microsoft.Office.Interop.Outlook.MailItem newMail = mail.Forward();

            log.Debug(
                  "Forwarding Email to  EMAIL to: Recipient: " + forwardRecipient +
                  "; Sender: " + mail.SenderName + "; Subject: " +
                  mail.Subject);

            newMail.Subject = forwardSubject;

            newMail.Recipients.Add(forwardRecipient);

            // Need to clear out email Body...
            newMail.Body = buildPhishingEmailBody(mail);

            // Need to attach the currMail item...
            newMail.Attachments.Add(
               mail, Microsoft.Office.Interop.Outlook.OlAttachmentType.olEmbeddeditem);

            // NO LONGER DISPLAYING THE EMAIL - BUT LEAVING HERE FOR NOW!
            // Display the new eMail and pass in TRUE to make it modal.  This
            // will require the user to have to respond to the new email that
            // has popped up onto the user's screen; either modify/not modify
            // and send it, or cancel it.  If the user cancel's it, the
            // application will treat that as a cancel of the Phishing
            // Reporting operation, and the email will neither be forwarded
            // nor moved.
            //newMail.Display(true);

            // Based on feedback from JV, he would rather just send the
            // forwarded email!
            // Send the forwarded email...
            newMail.Send();

            // No need to do this logic now since we are just sending it and
            // no longer displaying the forwarded email.  But leaving it here
            // for now.  We can clean this code up after we have source code
            // control.
            // Need to do something hokey here...capture exception to determine
            // if the mail was sent successfully...
            //try
            //{
            //   // Need to return if the email was cancelled.  Otherwise, we
            //   // will get an exception with the message "The item is moved or
            //   // deleted".
            //   mailSent = newMail.Sent;
            //}
            //catch (System.Exception ex)
            //{
            //   // The only way to tell if we got the expected exception we are
            //   // looking for is to verify the contents of the exception
            //   // message contains the "explanation" we expect.
            //   if (ex.Message.ToUpper().Contains(
            //      "THE ITEM HAS BEEN MOVED OR DELETED") == true)
            //   {
            //      mailSent = true;
            //   }
            //   else
            //   {
            //      // If the exception is another exception, just output an
            //      // error message!
            //      MessageBox.Show(
            //         "Exception encountered - Please report to IT!  " +
            //         "Exception: " +
            //         ex.Message);
            //   }
            //}
         }
      }

      private string buildPhishingEmailBody(Microsoft.Office.Interop.Outlook.MailItem mail)
      {
         string phishingEmailBody = "";

         // Get MAIL HEADER information...
         // Envelope Sender
         // Reply-To
         // From
         // Sender IP
         // Sender Domain

         string senderEmailAddress = null;
         string senderName = null;
         // The following value is semi-colon delimited...
         string replyRecipients = mail.ReplyRecipientNames;

         if (mail != null)
         {
            // Need to handle EXCHANGE versus SMTP email types.  If the
            // SenderEmailType is NULL, it is netiher an EXCHANGE or SMTP
            // originaed email.  In this case, it may be auto-generated by
            // a bot...in this case, just share the SenderName if available.
            if (mail.SenderEmailAddress == null)
            {
               senderEmailAddress = mail.SenderEmailAddress;
               senderName = mail.SenderName;
            }
            else if (mail.SenderEmailType.ToLower() == "ex")
            {
               Microsoft.Office.Interop.Outlook.AddressEntry sender =
                  mail.Sender;

               Microsoft.Office.Interop.Outlook.ExchangeUser exUser =
                  sender.GetExchangeUser();

               senderEmailAddress = exUser.PrimarySmtpAddress;
               senderName = mail.SenderName;
            }
            else if (mail.SenderEmailType.ToLower() == "smtp")
            {
               senderEmailAddress = mail.SenderEmailAddress;
               senderName = mail.SenderName;
            }

            log.Debug(
               "Metadata for Phishing Email Body:\n" +
               "Sender Name: " + senderName + "\n" +
               "Sender Email Address:  " + senderEmailAddress + "\n" +
               "Reply-To:  " + replyRecipients + "\n");

            Microsoft.Office.Interop.Outlook.PropertyAccessor outlookPA =
               mail.PropertyAccessor;

            const string TRANSPORT_MESSAGE_HEADERS_PROPERTY =
               "http://schemas.microsoft.com/mapi/proptag/0x007D001E";

            string mailHeaders = outlookPA.GetProperty(TRANSPORT_MESSAGE_HEADERS_PROPERTY);

            phishingEmailBody = "From: " + senderName;

            if (senderEmailAddress != null)
            {
               phishingEmailBody += " <" + senderEmailAddress + ">\n";
            }
            else
            {
               phishingEmailBody += "\n";
            }

            phishingEmailBody +=
               "Reply-To: " + replyRecipients + "\n" +
               "Headers:\n" + mailHeaders;
         }

         return phishingEmailBody;
      }

      /**
       * 
       * Sets an HCU register key value.
       * 
       */

      public static void createHCURegistryKeyDWordValue(
         string keyName,
         string valueName,
         object value,
         bool forceChangeIfExists)
      {
         RegistryKey rk = Registry.CurrentUser.OpenSubKey(keyName, true);

         try
         {
            if (rk == null)
            {
               log.Debug("Creating HCU Registry Key: " + keyName);

               rk = Registry.CurrentUser.CreateSubKey(keyName);
            }
            else
            {
               log.Debug("FOUND HCU Registry Key: " + keyName);
            }

            Object valueValue = rk.GetValue(valueName);

            if (valueValue == null)
            {
               log.Debug(
                  "Setting the Registry Key Value [" +
                  keyName + "\\" + valueName + "]: " + value);

               rk.SetValue(valueName, value, RegistryValueKind.DWord);
            }
            else
            {
               log.Debug(
                  "Registry Key Value ALREADY exists!  " +
                  "Skipping Setting of Registry Key Value [" +
                  keyName + "\\" + valueName + "]: " + value);
            }

            rk.Close();
         }
         catch (Exception ex)
         {
            log.Error("Exception found during Registry Key Value Creation: " +
               ex.Message);
         }
      }

      /**
       * 
       * Sets an HLM register key value.
       * 
       */

      public static void setHLMRegistryKeyValue(
         string keyName,
         string valueName,
         object value)
      {
         RegistryKey rk = Registry.CurrentUser.OpenSubKey(
            keyName, true);

         rk.SetValue(valueName, value);

         rk.Close();
      }

      /**
       * 
       * This method will retrieve an HKEY_CURRENT_USER boolean key value from
       * the registry.
       * 
       */
      public static object retrieveHCURegistryKeyValue(
         string keyName,
         string valueName,
         KeyValueTypes keyValueType)
      {
         object result = null;

         log.Debug("Getting HCU Registry Key Value [" + keyName + ", " + valueName + "]");

         RegistryKey rk = Registry.CurrentUser.OpenSubKey(
            keyName, false);

         // If the Registry Key Parent path is NOT FOUND, we will just return
         // false!
         if (rk != null)
         {
            object keyValue = rk.GetValue(valueName);

            if (keyValue != null)
            {
               if (keyValueType == KeyValueTypes.Boolean)
               {
                  result = Convert.ToBoolean(keyValue);
               }
               else if (keyValueType == KeyValueTypes.String)
               {
                  // Need to determine what type of word we are dealing with...
                  if (rk.GetValueKind(valueName) == RegistryValueKind.DWord)
                  {
                     result = Convert.ToString((Int32)keyValue);
                  }
                  else if (rk.GetValueKind(valueName) == RegistryValueKind.QWord)
                  {
                     result = Convert.ToString((Int64)keyValue);
                  }
                  else
                  {
                     result = keyValue;
                  }
               }
               else if (keyValueType == KeyValueTypes.Integer)
               {
                  result = int.Parse(keyValue.ToString());
               }
               else
               {
                  MessageBox.Show(
                     "We DO NOT support the Key Value Type: " +
                     keyValueType.ToString(),
                     "Unsupported Registry Key Value Type",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Warning);
               }
            }
            else
            {
               log.Debug("Registry Key Value is NULL!");
            }

            rk.Close();
         }
         else
         {
            MessageBox.Show(
               "HCU Registry Key [" +
               keyName + "\\" +
               valueName + "] NOT Found!",
               "Registry Key Not Found",
               MessageBoxButtons.OK,
               MessageBoxIcon.Warning);

            log.Warn("Registry Key Value NOT Found!");
         }

         log.Debug(
            "Value of HCU Registry Key Value [" +
            keyName + ", " + valueName + "] = " +
            result.ToString());

         return result;
      }

      /**
       * 
       * This method will retrieve an HKEY_LOCAL_MACHINE boolean key value from
       * the registry.
       * 
       */
      public static object retrieveHLMRegistryKeyValue(
         string keyName,
         string valueName,
         KeyValueTypes keyValueType)
      {
         object result = null;

         log.Debug("Getting HLM Registry Key Value [" + keyName + ", " + valueName + "]");

         RegistryKey rk = Registry.LocalMachine.OpenSubKey(
            keyName, false);

         // If the Registry Key Parent path is NOT FOUND, we will just return
         // false!
         if (rk != null)
         {
            object keyValue = rk.GetValue(valueName);

            if (keyValue != null)
            {
               if (keyValueType == KeyValueTypes.Boolean)
               {
                  result = Convert.ToBoolean(keyValue);
               }
               else if (keyValueType == KeyValueTypes.String)
               {
                  // Need to determine what type of word we are dealing with...
                  if (rk.GetValueKind(valueName) == RegistryValueKind.DWord)
                  {
                     result = Convert.ToString((Int32)keyValue);
                  }
                  else if (rk.GetValueKind(valueName) == RegistryValueKind.QWord)
                  {
                     result = Convert.ToString((Int64)keyValue);
                  }
                  else
                  {
                     result = keyValue;
                  }
               }
               else if (keyValueType == KeyValueTypes.Integer)
               {
                  result = int.Parse(keyValue.ToString());
               }
               else
               {
                  MessageBox.Show(
                     "We DO NOT support the Key Value Type: " +
                     keyValueType.ToString(),
                     "Unsupported Registry Key Value Type",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Warning);
               }
            }
            else
            {
               log.Debug("Registry Key Value is NULL!");
            }

            rk.Close();
         }
         else
         {
            MessageBox.Show(
               "HLM Registry Key [" +
               keyName + "\\" +
               valueName + "] NOT Found!",
               "Registry Key Not Found",
               MessageBoxButtons.OK,
               MessageBoxIcon.Warning);

            log.Warn("Registry Key Value NOT Found!");
         }

         log.Debug(
            "Value of HLM Registry Key Value [" +
            keyName + ", " + valueName + "] = " +
            result.ToString());

         return result;
      }
   }
}
