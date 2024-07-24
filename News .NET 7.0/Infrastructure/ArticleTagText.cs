using News.BLL.DTO;
using News.Models;

namespace News.Infrastructure
{
    public static class ArticleTagText
    {
        public static List<TagTextView> GetArticleTagText(ArticleDTO articleDTO)
        {
            List<TagTextView> tagTextList = new List<TagTextView>();
            List<string> tagsView = new List<string>()
            {
                "[image]",
                "[text]"
            };
            List<string> tagsViewEnds = new List<string>()
            {
                "[/image]",
                "[/text]"
            };

            TagTextView temp = new TagTextView();
            List<string> articleTextList = articleDTO.Text.Split(new string[] { " ", "\r\n" }, StringSplitOptions.None).ToList();

            List<FileDTO> articleFiles = articleDTO.Files.Where(a => a.Rank != "Title").OrderBy(a => a.Rank).ToList();

            int numbreArticleFiles = 0;

            foreach (var item in articleTextList)
            {
                if (tagsView.Contains(item))
                {
                    switch (item)
                    {
                        case "[text]":
                            temp.TagName = TagName.Text;
                            break;
                        case "[image]":
                            temp.TagName = TagName.Image;
                            temp.Value = articleFiles[numbreArticleFiles].Path;
                            numbreArticleFiles++;
                            break;
                    }
                }
                else if (tagsViewEnds.Contains(item))
                {
                    tagTextList.Add(temp);
                    temp = new TagTextView();
                }
                else
                {
                    temp.Value += item + " ";
                }
            }
            return tagTextList;
        }
    }
}
