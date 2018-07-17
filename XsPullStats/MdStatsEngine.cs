using System;
using System.Collections.Generic;
using System.Linq;
using MarkdownLog;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XsPullStats
{
    public class MdStatsEngine
    {
        //Identifiants du compte GitHub qui réalisera l'opération de Pull
        public String Username { get; set; }
        public String Password { get; set; }
        public String repoPath { get; set; }
        public String emailSignature { get; set; }
        const String MDNAME = "Git_Stats";

        public MdStatsEngine(String usr, String psw, String path, String emailAddress)
        {
            this.Username = usr;
            this.Password = psw;
            this.repoPath = path;
            this.emailSignature = emailAddress;
        }

        public void Create()
        {
            //Pull du repo
            LibGit2Sharp.PullOptions options = new LibGit2Sharp.PullOptions();
            options.FetchOptions = new FetchOptions();
            options.FetchOptions.CredentialsProvider = new CredentialsHandler(
                (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials()
                    {
                        Username = this.Username,
                        Password = this.Password
                    });

            //Emplacement absolu du repo local
            var repo = new LibGit2Sharp.Repository(this.repoPath);

            //Realisation de l'action de Pull avec le repo designé avant, l'Username du programme, et l'adresse mail associée
            MergeResult mergeResult = Commands.Pull(
                repo,
                new Signature(Username, this.emailSignature, DateTimeOffset.Now),
                options
            );

            //Traitement du resultat du Pull
            if (mergeResult.Status == MergeStatus.UpToDate)
            {
                //Si nous sommes à jour, on ne fait rien, et on le dit dans la console
                Console.WriteLine("On est deja a jour donc Commit a null");
            }
            else
            {
                //Sinon, on récupère le commit que l'on vient de Pull
                //On le compare au commit précédent (son parent)
                //Et on print les noms des fichiers modifiés 
                Console.WriteLine("Mise a jour");
                Console.WriteLine("Commit : " + mergeResult.Commit.Id.ToString());
                Tree commitTree = mergeResult.Commit.Tree;
                Tree parentCommitTree = mergeResult.Commit.Parents.Single().Tree;

                TreeChanges changes = repo.Diff.Compare<TreeChanges>(parentCommitTree, commitTree);

                //Console.WriteLine("{0} files changed :", changes.Count());
                List<FileData> fDatas = new List<FileData>();
                //
                var patch = repo.Diff.Compare<Patch>(parentCommitTree, commitTree);
                //
                createFileDatas(changes, fDatas, patch);
                /* Affichage des noms des fichiers modifiés
                Console.WriteLine("Files :");
                foreach (String file in fileNames)
                {
                    Console.WriteLine("{0}", file);
                }
                */

                var maTable = fDatas.ToMarkdownTable();
                Console.WriteLine(maTable);
                //
                processMyTableToMdTable(maTable);
            }
            Console.ReadLine();
        }

        private void createFileDatas(TreeChanges changes, List<FileData> fDatas, Patch patch)
        {
            //Creation des objets FileData en initialisant le path et le status
            foreach (TreeEntryChanges treeEntryChanges in changes)
            {
                String[] split = treeEntryChanges.Path.Split('\\');
                //fileNames.Add(split[split.Length - 1]);
                fDatas.Add(new FileData()
                {
                    Path = split[split.Length - 1],
                    status = treeEntryChanges.Status.ToString(),
                    linesAdded = 0,
                    linesDeleted = 0
                });
            }
            //Puis init des lignes ajoutées et supprimées pour chaque fichier
            for (int i = 0; i < fDatas.Count; i++)
            {
                FileData f = fDatas[i];
                var pt = patch.Where(x => x.Path == f.Path);
                if (pt.Count<PatchEntryChanges>() > 0)
                {
                    f.linesAdded = pt.ElementAt<PatchEntryChanges>(0).LinesAdded;
                    f.linesDeleted = pt.ElementAt<PatchEntryChanges>(0).LinesDeleted;
                }

            }
        }

        public void processMyTableToMdTable(Table maTable)
        {
            String dayDate = (DateTime.Now.Day.ToString()) + "-" + (DateTime.Now.Month.ToString()) + "-" + (DateTime.Now.Year.ToString());

            FileStream md = new FileStream(this.repoPath + MdStatsEngine.MDNAME + "_" + dayDate + ".md", FileMode.OpenOrCreate);
            StreamWriter sr = new StreamWriter(md);
            sr.WriteLine("");
            sr.WriteLine("");
            sr.WriteLine("");
            String[] hourWithMili = DateTime.Now.TimeOfDay.ToString().Split('.');
            var hourOfTheDay = hourWithMili[0].ToMarkdownHeader();
            sr.WriteLine(hourOfTheDay);
            //
            //Deplacement à la fin du fichier pour ne pas écraser le contenu au cas où on écrit plusieurs fois par jour
            md.Seek(0, SeekOrigin.End);
            //
            StringReader read = new StringReader(maTable.ToMarkdown());
            do
            {
                String line = read.ReadLine();
                if (line != null)
                {
                    if (line.Length >= 5)
                        sr.WriteLine(line.Substring(5));
                    else
                        sr.WriteLine(line);
                }
                else
                    break;
            } while (true);
            //
            read.Close();
            sr.Close();
            md.Close();
        }
    }
}
