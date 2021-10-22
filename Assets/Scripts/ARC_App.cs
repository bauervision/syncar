
///<summary>Extends the ARC_Library class with all of the specifc details pertaining to the actual app. </summary>
[System.Serializable]
public class ARC_App : ARC_Library
{
    #region Specific App Members

    #endregion

    public ARC_App()
    {
        // handle all inherited members
        this.createdBy = string.Empty;
        this.description = string.Empty;
        this.examples = false;
        this.id = string.Empty;//will be replaced by server guid when we push to database
        this.level = 0;
        this.subtitle = string.Empty;
        this.title = string.Empty;
        this.userAdded = false;
        // now handle specific app members
    }


    public ARC_App(ARC_Library libraryData)
    {
        // handle all inherited members
        this.createdBy = libraryData.createdBy;
        this.description = libraryData.description;
        this.examples = libraryData.examples;
        this.id = string.Empty;//will be replaced by server guid when we push to database
        this.level = libraryData.level;
        this.subtitle = libraryData.subtitle;
        this.title = libraryData.title;
        this.userAdded = false;
        // now handle specific app members

    }
}
