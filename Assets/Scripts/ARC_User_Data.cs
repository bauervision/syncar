
[System.Serializable]
public class ARC_Package
{
    ///<summary>Specific key which identifies the actual asset package within "/content" in the database </summary>
    public string key;

    ///<summary>Title of this package </summary>
    public string title;
}


///<summary>Data which associates the user with the actual content data that can be used in the app </summary>
[System.Serializable]
public class ARC_Library
{
    ///<summary>Who created this content? If the username here matches the user's username, 
    ///then this content will be filed under "My App"</summary>    
    public string createdBy;

    ///<summary>Is this an official example?</summary>  
    public bool examples;

    ///<summary>Brief description about what this content does</summary>  
    public string description;

    ///<summary>The GUID of this content, used to locate the actual data to be loaded </summary>  
    public string id;

    ///<summary>What subscription level is this content designed for? Intro, Advanced, Premium, Partner</summary>  
    public int level;

    ///<summary>Brief additional heading </summary>  
    public string subtitle;

    ///<summary>Title given to this content </summary>  
    public string title;

    ///<summary>Used on the website only</summary>  
    public bool userAdded;
}

[System.Serializable]
public class ARC_User_Data
{
    ///<summary>Timestamp when this user was created  </summary>  
    public string createdOn;

    ///<summary>Email used to join ARCreate</summary>  
    public string email;

    ///<summary>Specific GUID of this user</summary>  
    public string id;

    ///<summary>Last time the user specifically signed in</summary>  
    public string lastSignInTime;

    ///<summary>An Array of all the ARCreate content the user has access to, including free community content,
    /// offical examples released by the company, and the user's own personal apps.</summary>  
    public ARC_Library[] library;

    ///<summary>User's name  </summary>  
    public string name;

    ///<summary>  </summary>
    public ARC_Package[] packages;

    ///<summary>User's role: NOVICE, EXPERT, ADMIN</summary>  
    public int role;

    ///<summary>User's subscription level: INTRO, ADVANCED, PREMIUM, PARTNER  </summary>  
    public int subscriptionLevel;

    ///<summary>Username of the user  </summary>  
    public string username;

    ///<summary>4 digit pin used to quickly sign in to their apps.  </summary>  
    public string userpin;



    public ARC_User_Data(string email, string userId, string name, ulong date)
    {
        this.createdOn = date.ToString();
        this.email = email;
        this.id = userId;
        this.lastSignInTime = date.ToString();
        this.name = name;
        this.role = 0;
        this.subscriptionLevel = 0;
        this.username = "NewARCreateUser";
        this.userpin = "1234";
    }


}