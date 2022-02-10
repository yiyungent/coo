using coo.Models.UStar;
using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace coo.Services
{
    public class UStarService
    {
        /// <summary>
        /// 此用户 star 此仓库了吗?
        /// </summary>
        /// <param name="userName">此用户</param>
        /// <param name="repoUserName">仓库对应 userName</param>
        /// <param name="repoName">仓库名</param>
        /// <returns></returns>
        public async Task<bool> Star(string userName, string repoUserName, string repoName, string githubToken, bool githubAction)
        {
            bool rtn = false;
            string githubStargazersUrl = $"https://api.github.com/repos/{repoUserName}/{repoName}/stargazers";

            try
            {
                // https://docs.github.com/en/rest/overview/resources-in-the-rest-api#oauth2-token-sent-in-a-header
                // https://github.com/settings/tokens/new
                // https://blog.cyfan.top/p/348e7d8a.html
                // 注意: 记得 Github API 不允许空 User-Agent，所以你还得在header中加入UA
                string[] headers = new string[] {
                    "user-agent: GoogleChrome",
                    "Content-Type: application/json",
                    $"Authorization: token {githubToken}"
                };
                string jsonStr = Utils.HttpUtil.HttpGet(url: githubStargazersUrl, headers: headers);
                List<GitHubStargazersUrlResponseModel.GitHubStargazersUrlItemModel> jsonModel =
                    Utils.JsonUtil.JsonStr2Obj<List<GitHubStargazersUrlResponseModel.GitHubStargazersUrlItemModel>>(jsonStr);

                foreach (var item in jsonModel)
                {
                    if (item.login == userName)
                    {
                        rtn = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                rtn = false;
                Utils.LogUtil.Exception(ex);
            }

            if (githubAction)
            {
                Utils.GitHubActionsUtil.SetOutput("coo_ustar", rtn.ToString());
            }

            return await Task.FromResult(rtn);
        }

        public async Task<bool> Star(string userName, string repoFullName, string githubToken, bool githubAction)
        {
            string[] userAndRepo = repoFullName.Split("/", StringSplitOptions.RemoveEmptyEntries);

            return await Star(userName, userAndRepo[0], userAndRepo[1], githubToken, githubAction);
        }
    }
}
