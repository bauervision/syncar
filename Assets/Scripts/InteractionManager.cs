using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation.Samples;
public class InteractionManager : MonoBehaviour
{
    public static InteractionManager instance;

    [Header("User Form Data")]
    public string user_name = string.Empty;
    public string email = string.Empty;
    public string password = string.Empty;
    public string username = string.Empty;
    public Text EmailErrorText;
    public Text PasswordErrorText;
    public GameObject FormErrorText;


    [Header("Key UI GameObjects")]
    public GameObject StartScreen;
    public GameObject GameScreen;
    public GameObject InfoScreen;
    public GameObject LoginForm;
    public GameObject SignUpForm;
    public GameObject SigningInPanel;
    public GameObject SideToolBar;
    public Button LoadAppButton;
    public Button SaveAppButton;
    public Button PublishAppButton;


    [Header("Info Tab Panels")]
    public GameObject AccountPanel;
    public GameObject AssetPanel;
    public GameObject PublishPanel;

    [Header("App Panels")]
    public Text AR_Title_Text;
    public Text selectedAppText;
    public string publishAppTitle;
    public string publishAppSubtitle;
    public int publishAppLevel;



    [Header("Data Panels")]
    public GameObject NewButton;
    public GameObject TopPanel;
    public GameObject Toast;
    public Text ToastMessage;
    public PlaceOnPlane placeOnPlane;


    [Header("User Data Fields")]
    public Text username_Text;
    public Text createdOn_Text;
    public Text email_Text;
    public Text role_Text;
    public Text subscription_Text;
    public Image userIcon;
    public Sprite[] icons;
    public Dropdown myAppsDropdown = null;
    public Dropdown packagesDropdown = null;
    public Dropdown libraryDropdown = null;
    public Dropdown subscriptionLevelDropdown = null;


    #region Private members
    private bool isAppLoaded = false;
    private List<string> dd_options_apps = new List<string>();
    private List<string> dd_options_packages = new List<string>();
    private List<string> dd_options_library = new List<string>();
    private List<string> dd_options_subscriptionLevel = new List<string>();

    private string[] subscriptionLevels = new string[] { "Intro", "Advanced", "Premium", "Partner" };


    #endregion

    private void Start()
    {
        instance = this;

        // make sure app starts in portrait mode for login screens
        Screen.orientation = ScreenOrientation.Portrait;
        StartScreen.SetActive(true);
        GameScreen.SetActive(false);
        SignUpForm.SetActive(false);
        InfoScreen.SetActive(false);
        TopPanel.SetActive(false);
        Toast.SetActive(false);
        placeOnPlane.enabled = false;
        EmailErrorText.text = "";
        PasswordErrorText.text = "";
        FormErrorText.SetActive(false);
        SigningInPanel.SetActive(false);
        selectedAppText.text = "";
        LoadAppButton.interactable = false;
        SaveAppButton.interactable = false;
        PublishAppButton.interactable = false;
        SideToolBar.SetActive(false);
    }

    private void Update()
    {
        if (publishAppTitle != string.Empty && publishAppSubtitle != string.Empty)
        {
            // make sure we only set them to true once
            if (!SaveAppButton.interactable && !PublishAppButton.interactable)
            {
                SaveAppButton.interactable = true;
                PublishAppButton.interactable = true;
            }
        }
        else
        {
            // only set them back to false once
            if (SaveAppButton.interactable && PublishAppButton.interactable)
            {
                SaveAppButton.interactable = false;
                PublishAppButton.interactable = false;
            }
        }
    }

    #region Public UI Methods

    public void Set_Publish_App_Title(string title)
    {
        publishAppTitle = title;
    }

    public void Set_Publish_App_Subtitle(string subtitle)
    {
        publishAppSubtitle = subtitle;
    }

    public void Set_Selected_App(int appIndex)
    {
        selectedAppText.text = (appIndex > 0) ? dd_options_apps[appIndex] : string.Empty;
        LoadAppButton.interactable = (appIndex > 0);
    }

    public void Set_Selected_LibraryApp(int appIndex)
    {
        print(appIndex);
        selectedAppText.text = (appIndex > 0) ? dd_options_library[appIndex] : string.Empty;
        LoadAppButton.interactable = (appIndex > 0);
    }

    public void Loaded_Selected_App()
    {

        //now start handling the app load
        isAppLoaded = true;
        ToggleInfoScreen();
        placeOnPlane.enabled = true;
        GameScreen.SetActive(true);
        NewButton.transform.GetChild(0).transform.gameObject.SetActive(false);
        NewButton.transform.GetChild(1).transform.gameObject.SetActive(true);


        // find which app in the library we have decided on
        ARC_App loadedApp;
        foreach (ARC_Library libraryApp in FirebaseManager.instance.thisUser.library)
            if (libraryApp.title == selectedAppText.text)
            {
                loadedApp = new ARC_App(libraryApp);
                ARC_App_Manager.instance.current_app = loadedApp;
                // finally set the UI based on the loaded app
                AR_Title_Text.text = loadedApp.title;
                StartCoroutine(DisplayToast(loadedApp.title + " loaded!"));
            }


    }


    #region UI Navigation
    ///<summary>Called from the Bottom Toolbar wherein a user wants to initiate a new app, or save the current one </summary>
    public void Create_New_App()
    {
        placeOnPlane.enabled = false;

        // if no app is loaded, this is a "New" button
        if (!isAppLoaded)
        {
            //create one with default values
            ARC_App newApp = new ARC_App();
            //overwrite any possible loaded app
            ARC_App_Manager.instance.current_app = newApp;

            isAppLoaded = true;
            InfoScreen.SetActive(true);
            Set_Publish_Panel();
            NewButton.transform.GetChild(0).transform.gameObject.SetActive(false);
            NewButton.transform.GetChild(1).transform.gameObject.SetActive(true);


        }
        else
        {
            // becomes a Save button
            SaveFile();
        }
    }



    public void Set_Account_Panel()
    {
        AccountPanel.SetActive(true);
        AssetPanel.SetActive(false);
        PublishPanel.SetActive(false);
    }

    public void Set_Asset_Panel()
    {
        AccountPanel.SetActive(false);
        AssetPanel.SetActive(true);
        PublishPanel.SetActive(false);
    }

    public void Set_Publish_Panel()
    {
        AccountPanel.SetActive(false);
        AssetPanel.SetActive(false);
        PublishPanel.SetActive(true);
    }

    public void ReturnToLoginScreen()
    {
        LoginForm.SetActive(true);
        SignUpForm.SetActive(false);
    }
    public void ReturnToSignUpScreen()
    {
        LoginForm.SetActive(false);
        SignUpForm.SetActive(true);
    }

    public void ToggleInfoScreen()
    {
        InfoScreen.SetActive(!InfoScreen.activeInHierarchy);

        if (!InfoScreen.activeInHierarchy)
        {
            placeOnPlane.enabled = true;

            if (isAppLoaded)
            {
                TopPanel.SetActive(true);
                SideToolBar.SetActive(true);
            }
        }
        else placeOnPlane.enabled = false;
    }

    #endregion

    #region Form Data
    public void SetEmail(string formEmail)
    {
        email = formEmail;
        EmailErrorText.text = "";
    }
    public void SetPassword(string formPassword)
    {
        password = formPassword;
        PasswordErrorText.text = "";
    }

    public void SetUserName(string formUserName) { username = formUserName; }


    #endregion


    #region General Methods
    public void Login()
    {
        if (email != string.Empty && password != string.Empty)
        {
            if (!SigningInPanel.activeInHierarchy)
                SigningInPanel.SetActive(true);

            StartScreen.SetActive(false);
            FirebaseManager.LoginReturningUser(email, password);

        }
        else
        {
            EmailErrorText.text = (email == string.Empty) ? "Missing..." : "";
            PasswordErrorText.text = (password == string.Empty) ? "Missing..." : "";
        }

    }

    public void LogOut()
    {
        FirebaseManager.instance.SignOut();
    }

    public void HandleError(string message)
    {
        FormErrorText.SetActive(true);
        FormErrorText.GetComponent<Text>().text = message;
    }

    public void HandleLogin(string message)
    {
        // fire off the welcome toast
        StartCoroutine(DisplayToast(message));

        // now set all the text fields
        username_Text.text = FirebaseManager.instance.thisUser.username;
        createdOn_Text.text = FirebaseManager.instance.thisUser.createdOn;
        email_Text.text = FirebaseManager.instance.thisUser.email;
        role_Text.text = GetUserRoleText(FirebaseManager.instance.thisUser.role);
        subscription_Text.text = GetSubscriptionLevelText(FirebaseManager.instance.thisUser.subscriptionLevel);
        userIcon.sprite = icons[FirebaseManager.instance.thisUser.role];

        SetupDropDowns();

        if (FirebaseManager.instance.thisUser.role > 0)
        {
            NewButton.GetComponent<Button>().enabled = true;
            NewButton.transform.GetChild(0).transform.gameObject.SetActive(true);
            NewButton.transform.GetChild(1).transform.gameObject.SetActive(false);
        }
        else//user is a novice so they cant save anything
        {
            NewButton.GetComponent<Button>().enabled = false;
            NewButton.transform.GetChild(0).transform.gameObject.SetActive(false);
            NewButton.transform.GetChild(1).transform.gameObject.SetActive(false);
        }


    }

    private void SetupDropDowns()
    {
        //prep work
        dd_options_apps.Clear();
        myAppsDropdown.ClearOptions();
        dd_options_packages.Clear();
        packagesDropdown.ClearOptions();
        dd_options_library.Clear();
        libraryDropdown.ClearOptions();
        dd_options_subscriptionLevel.Clear();
        subscriptionLevelDropdown.ClearOptions();

        // populate the dropdown options from the library
        if (FirebaseManager.instance.thisUser.library != null)
        {
            dd_options_apps.Add("Select an App...");
            dd_options_library.Add("Select from Library...");
            foreach (ARC_Library item in FirebaseManager.instance.thisUser.library)
            {
                // if this item was created by the user...
                if (item.createdBy == FirebaseManager.instance.thisUser.username)
                    dd_options_apps.Add(item.title);
                else // this is from the general library
                {

                    // if this is an offical example
                    if (item.examples)
                    {
                        dd_options_library.Add(item.title);
                        //TODO: signify its different somehow
                    }
                    else//public content
                        dd_options_library.Add(item.title);
                }
            }
        }
        else
        {
            dd_options_apps.Add("Nothing Yet!");
            dd_options_apps.Add("Library Empty");
        }


        // handle the subscription dropdown
        for (int i = 0; i <= FirebaseManager.instance.thisUser.subscriptionLevel; i++)
            dd_options_subscriptionLevel.Add(subscriptionLevels[i]);


        // handle the packages
        if (FirebaseManager.instance.thisUser.packages != null)
            foreach (ARC_Package package in FirebaseManager.instance.thisUser.packages)
                dd_options_packages.Add(package.title);
        else
            dd_options_packages.Add("No Packages Yet!");

        // now add the options to the dropdown menus
        myAppsDropdown.AddOptions(dd_options_apps);
        libraryDropdown.AddOptions(dd_options_library);
        subscriptionLevelDropdown.AddOptions(dd_options_subscriptionLevel);
        packagesDropdown.AddOptions(dd_options_packages);

    }

    #endregion

    #region Utilities
    private string GetUserRoleText(int role)
    {
        switch (role)
        {
            case 0: return "Novice";
            case 1: return "Expert";
            default: return "Expert";
        }
    }

    private string GetSubscriptionLevelText(int level)
    {
        switch (level)
        {
            case 0: return "Intro";
            case 1: return "Advanced";
            case 2: return "Premium";
            default: return "Partner";
        }
    }

    #endregion

    public void HandleLogOut()
    {
        if (!StartScreen.activeInHierarchy)
            StartScreen.SetActive(true);


        if (!LoginForm.activeInHierarchy)
            LoginForm.SetActive(true);

        EmailErrorText.text = "";
        PasswordErrorText.text = "";
        FormErrorText.GetComponent<Text>().text = "";

        if (FormErrorText.activeInHierarchy)
            FormErrorText.SetActive(false);

        StartCoroutine(DisplayToast("User has been signed out!"));
    }

    public void SignUp()
    {
        print("...handle signup verification...");
    }

    #endregion

    ///<summary>Called from the bottom menu, when the user hits the Save Icon, and from the Info Screen "Save Current App" button  </summary>
    public void SaveFile()
    {
        // make sure for whatever reason we have an app loaded to save
        if (ARC_App_Manager.instance.current_app != null)
        {

            SetLoadedAppData();
            // push date to firebase
            FirebaseManager.instance.SaveApp(ARC_App_Manager.instance.current_app);
            StartCoroutine(DisplayToast("File Saved!"));
        }
        else
        {
            Debug.LogError("No App loaded to save!!");
        }
    }

    private void SetLoadedAppData()
    {
        ARC_User_Data currentUser = FirebaseManager.instance.thisUser;
        ARC_App currentApp = ARC_App_Manager.instance.current_app;

        currentApp.createdBy = currentUser.username;
        currentApp.description = "";
        currentApp.examples = (currentUser.role >= 2);
        currentApp.id = "";// will get from server later
        currentApp.level = publishAppLevel;
        currentApp.subtitle = publishAppSubtitle;
        currentApp.title = publishAppTitle;
    }
    private IEnumerator DisplayToast(string message)
    {
        placeOnPlane.enabled = false;
        Toast.SetActive(true);
        ToastMessage.text = message;
        yield return new WaitForSeconds(2.0f);
        Toast.SetActive(false);
        placeOnPlane.enabled = true;
    }

}