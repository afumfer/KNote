using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository;

public class KntRepositoryBase
{    
    protected internal readonly RepositoryRef _repositoryRef;

    public KntRepositoryBase(RepositoryRef repositoryRef)
    {
        _repositoryRef = repositoryRef;
    }

    #region Utils methods for repositories

    protected int ExtractNoteNumberSearch(string textSearch)
    {
        if (string.IsNullOrEmpty(textSearch?.Trim()))
            return 0;

        int n = 0;
        string strStartNumber;

        if (textSearch[0] == '#')
        {
            var i = textSearch.IndexOf(' ', 0);
            if (i > 0)
                strStartNumber = textSearch.Substring(1, i - 1);
            else
                strStartNumber = textSearch.Substring(1, textSearch.Length - 1);
            int.TryParse(strStartNumber, out n);
        }

        return n;
    }

    protected List<string> ExtractListTokensSearch(string textIn)
    {
        List<string> tokens = new List<string>();
        int i = 0, lenString = 0;
        int state = 0;
        char c;
        string word = "";
        char action = 'a';
        string especialToken = "";

        if (textIn == null)
            return tokens;

        lenString = textIn.Length;

        while (i < lenString)
        {
            c = textIn[i];

            switch (c)
            {
                case '\"':
                    if (state == 0)
                    {
                        state = 1;
                        action = 'p';
                    }
                    else
                    {
                        state = 0;
                        action = 'p';
                    }
                    break;
                case ' ':
                    if (state == 0 || state == 2)
                        action = 'p';
                    break;
                default:
                    action = 'a';
                    break;
            }

            if (action == 'p')
            {
                if (word != "")
                {
                    // Si es un Token especial => va con la siguiente palabra
                    if (word == "!")
                        especialToken = word;
                    else
                    {
                        word = especialToken + word;
                        especialToken = "";
                        tokens.Add(word);
                    }
                }
                word = "";
            }
            if (action == 'a')
                word += c;

            i++;
        }

        if (word != "")
            tokens.Add(word);

        return tokens;
    }

    protected void LoadChilds(FolderDto folder, List<FolderDto> allFolders)
    {
        folder.ChildFolders = allFolders.Where(fi => fi.ParentId == folder.FolderId)
            .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

        foreach (FolderDto f in folder.ChildFolders)
            LoadChilds(f, allFolders);
    }

    #endregion
}
