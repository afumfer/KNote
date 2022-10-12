using KNote.Model;
using KNote.Service;

namespace KntRedminePlugin
{
    public class KntRedminePluginCommand : IPluginCommand
    {
        public string Name 
        { 
            get 
            { 
                return "Name"; 
            } 
        } 
    
        public string Description
        {
            get
            {
                return "Description";
            }
        }

        public int Execute()
        {
            var f = new RedminePluginForm();
            f.Text = Description;
            f.Show();
            return 0;
        }
    }

}
