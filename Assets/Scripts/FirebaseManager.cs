using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System.Linq;
using System;
using MPUIKIT;
using UnityEngine.Events;
using UnityEngine.UI;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;
    public UnityEvent OnFirebaseInitialized = new UnityEvent();
    public UnityEvent OnActiveToken = new UnityEvent();
    public UnityEvent UserDataLoaded = new UnityEvent();
    public UnityEvent UserSignedOut = new UnityEvent();
    public bool userSignedIn = false;
    public ARC_User_Data thisUser = null;

    private Firebase.FirebaseApp app;
    private FirebaseDatabase AP_ArCreateDB;
    private Firebase.Auth.FirebaseUser user = null;

    #region Firebase Related
    protected Firebase.Auth.FirebaseAuth auth;
    private FirebaseDatabase firebaseDB;
    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth =
          new Dictionary<string, Firebase.Auth.FirebaseUser>();

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.Log("Failed to initialize Firebase" + task.Exception);

                return;
            }


            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
                OnFirebaseInitialized.Invoke();

            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);

            }
        });
    }

    protected void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        firebaseDB = FirebaseDatabase.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        auth.IdTokenChanged += IdTokenChanged;

        AuthStateChanged(this, null);
    }

    //Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;

        if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out user);
        if (senderAuth == auth && senderAuth.CurrentUser != user)
        {
            bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
                UserSignedOut.Invoke();
                thisUser = null;
                InteractionManager.instance.HandleLogOut();
            }
            user = senderAuth.CurrentUser;
            userByAuth[senderAuth.App.Name] = user;
            if (signedIn)
            {
                userSignedIn = true;
                OnActiveToken.Invoke();
                StartCoroutine(FetchPlayerData(user.UserId));
            }
        }
    }

    //Track ID token changes.
    void IdTokenChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        // if (senderAuth == auth && senderAuth.CurrentUser != null)
        // {
        //     senderAuth.CurrentUser.TokenAsync(false).ContinueWithOnMainThread(
        //       task => Debug.Log(String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
        // }
    }

    public static void SendNewRegistration(string name, string email, string password)
    {
        instance.StartCoroutine(instance.RegisterUser(name, email, password));
    }

    public static void LoginReturningUser(string email, string password)
    {
        instance.StartCoroutine(instance.LoginUser(email, password));
    }



    private IEnumerator RegisterUser(string name, string email, string password)
    {
        var auth = FirebaseAuth.DefaultInstance;
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => registerTask.IsCompleted);

        // at this point, we either have a successful reg or an issue, so lets handle both cases
        if (registerTask.Exception != null)
        {
            print($"Failed to register user: {registerTask.Exception}");
            // do something to the UI with the failure
            //RegistrationFlow.FailedRegistration(email);

        }
        else
        {
            print($"Successfully registered user: {registerTask.Result.Email}");
            // and now set up the initial playerdata
            string newID = registerTask.Result.UserId;
            string newEmail = registerTask.Result.Email;
            ulong joinedOn = registerTask.Result.Metadata.CreationTimestamp;
            // store player data locally
            thisUser = new ARC_User_Data(email, newID, name, joinedOn);
            // write the new registration data
            SavePlayerRegistration(thisUser);
            // do something to the UI for the success
            //RegistrationFlow.SuccessfulRegistration(name);
        }
    }

    private IEnumerator LoginUser(string email, string password)
    {
        var auth = FirebaseAuth.DefaultInstance;
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        // at this point, we either have a successful login or an issue, so lets handle both cases
        if (loginTask.Exception != null)
        {
            // do something to the UI with the failure.
            InteractionManager.instance.HandleError(loginTask.Exception.Message);

        }
        else
        {
            // fire off the data fetch
            FetchPlayerData(loginTask.Result.UserId);
        }
    }

    private IEnumerator FetchPlayerData(string userId)
    {
        InteractionManager.instance.SigningInPanel.SetActive(true);

        if (InteractionManager.instance.LoginForm.activeInHierarchy)
            InteractionManager.instance.LoginForm.SetActive(false);


        var loadPlayerDataTask = LoadPlayer(userId);
        yield return new WaitUntil(() => loadPlayerDataTask.IsCompleted);
        InteractionManager.instance.SigningInPanel.SetActive(false);

        // now that we have the data, save it locally
        if (loadPlayerDataTask.Result != null)
        {
            thisUser = loadPlayerDataTask.Result;
            UserDataLoaded.Invoke();
            // launch the toast
            InteractionManager.instance.HandleLogin($"Welcome Back {thisUser.name}");
        }
        else
        {
            print("Data is null, trying again...");
            StartCoroutine(FetchPlayerData(userId));
        }
    }


    // called from the app
    public void SaveApp(ARC_App currentAppData)
    {
        print("Saving..." + JsonUtility.ToJson(currentAppData));
        //await instance.firebaseDB.GetReference($"ar-create/users/{currentPlayerData.id}").SetRawJsonValueAsync(JsonUtility.ToJson(currentPlayerData));
    }

    // called during registration
    public void SavePlayerRegistration(ARC_User_Data currentPlayerData)
    {
        firebaseDB.GetReference($"ar-create/users/{currentPlayerData.id}").SetRawJsonValueAsync(JsonUtility.ToJson(currentPlayerData));
    }

    public static async Task<ARC_User_Data> LoadPlayer(string userId)
    {
        var dbSnapshot = await instance.firebaseDB.GetReference($"ar-create/users/{userId}").GetValueAsync();
        if (!dbSnapshot.Exists)
        {
            return null;
        }

        // return the resulting data
        return JsonUtility.FromJson<ARC_User_Data>(dbSnapshot.GetRawJsonValue());
    }


    // Sign out the current user.
    public void SignOut()
    {
        Debug.Log("Signing out.");
        auth.SignOut();

    }

    // Send a password reset email to the current email address.
    protected void SendPasswordResetEmail(string email)
    {
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread((authTask) =>
        {
            if (LogTaskCompletion(authTask, "Send Password Reset Email"))
            {
                Debug.Log("Password reset email sent to " + email);
            }
        });
    }

    protected bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            Debug.Log(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            Debug.Log(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string authErrorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    authErrorCode = String.Format("AuthError.{0}: ",
                      ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                }
                Debug.Log(authErrorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            Debug.Log(operation + " completed");
            complete = true;
        }
        return complete;
    }
}
