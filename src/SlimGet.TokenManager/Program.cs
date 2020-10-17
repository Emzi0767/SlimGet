using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Emzi0767;
using Emzi0767.Utilities;
using Microsoft.EntityFrameworkCore;
using SlimGet.Data;
using SlimGet.Data.Configuration;
using SlimGet.Data.Database;
using SlimGet.Services;

namespace SlimGet.Tools
{
    public sealed class Program
    {
        public static void Main(string[] args)
        {
            var configEnv = Environment.GetEnvironmentVariable("SLIMGET__CONFIGURATION") ?? "slimget.json";
            Console.WriteLine("Loading config from '{0}'", configEnv);

            var json = "{}";
            using (var fs = File.OpenRead(configEnv))
            using (var sr = new StreamReader(fs))
                json = sr.ReadToEnd();

            var config = JsonSerializer.Deserialize<SlimGetConfiguration>(json);

            Console.WriteLine("Executing configuration utility");
            var program = new Entrypoint(config);
            var async = new AsyncExecutor();
            async.Execute(program.ExecuteProgramAsync(args));
        }
    }

    public sealed class Entrypoint
    {
        private static string[] Entities { get; } = new[] { "user", "token" };
        private static string[][] Operations { get; } = new[] {
            new[] { "list", "create", "delete" },
            new[] { "list", "issue", "revoke" }
        };

        private SlimGetContext Database { get; }
        private TokenService Tokens { get; }

        public Entrypoint(SlimGetConfiguration config)
        {
            this.Database = new SlimGetContext(
                ConnectionStringProvider.Create(config.Storage.Database));

            this.Tokens = TokenService.Create(config.Security);
        }

        public async Task ExecuteProgramAsync(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Too few arguments supplied");
                this.PrintUsage(UsageKind.General);
                return;
            }

            var eid = Array.IndexOf(Entities, args[0]);
            if (eid < 0)
            {
                Console.WriteLine("Incorrect entity supplied");
                this.PrintUsage(UsageKind.General);
                return;
            }

            var opid = Array.IndexOf(Operations[eid], args[1]);
            if (opid < 0)
            {
                Console.WriteLine("Incorrect operation supplied");
                this.PrintUsage(eid == 0 ? UsageKind.User : UsageKind.Token);
                return;
            }

            if (eid == 0) // user
            {
                if (opid == 0) // list
                {
                    if (args.Length == 2)
                        this.ListUsers();
                    else
                        this.PrintUsage(UsageKind.UserList);
                }
                else if (opid == 1) // create
                {
                    if (args.Length != 4)
                        this.PrintUsage(UsageKind.UserCreate);
                    else
                        await this.CreateUserAsync(args[2], args[3]);
                }
                else if (opid == 2) // delete
                {
                    if (args.Length != 3)
                        this.PrintUsage(UsageKind.UserDelete);
                    else
                        await this.DeleteUserAsync(args[2]);
                }
            }
            else if (eid == 1) // token
            {
                if (opid == 0) // list
                {
                    if (args.Length != 3)
                        this.PrintUsage(UsageKind.TokenList);
                    else
                        await this.ListTokensAsync(args[2]);
                }
                else if (opid == 1) // issue
                {
                    if (args.Length != 3)
                        this.PrintUsage(UsageKind.TokenIssue);
                    else
                        await this.IssueTokenAsync(args[2]);
                }
                else if (opid == 2) // revoke
                {
                    if (args.Length != 3)
                        this.PrintUsage(UsageKind.TokenRevoke);
                    else
                        await this.RevokeTokenAsync(args[2]);
                }
            }
        }

        private void ListUsers()
        {
            Console.WriteLine("Registered users:");
            foreach (var userId in this.Database.Users.Select(x => x.Id))
                Console.WriteLine("  {0}", userId);
        }

        private async Task CreateUserAsync(string username, string email)
        {
            if (!username.All(IsValidUsernameCharacter))
            {
                Console.WriteLine("Invalid username supplied");
                this.PrintUsage(UsageKind.UserCreate);
                return;
            }

            try
            {
                _ = new MailAddress(email);
            }
            catch
            {
                Console.WriteLine("Invalid email address supplied");
                this.PrintUsage(UsageKind.UserCreate);
                return;
            }

            var dbuser = await this.Database.Users.FirstOrDefaultAsync(x => x.Id == username);
            if (dbuser != null)
            {
                Console.WriteLine("User with specified username already exists");
                this.PrintUsage(UsageKind.UserCreate);
                return;
            }

            dbuser = new User
            {
                Id = username,
                Email = email
            };
            await this.Database.Users.AddAsync(dbuser);
            await this.Database.SaveChangesAsync();

            Console.WriteLine("User created successfully");
        }

        private async Task DeleteUserAsync(string username)
        {
            var dbuser = await this.Database.Users.FirstOrDefaultAsync(x => x.Id == username);
            if (dbuser == null)
            {
                Console.WriteLine("User with specified username does not exist");
                this.PrintUsage(UsageKind.UserDelete);
                return;
            }

            this.Database.Users.Remove(dbuser);
            await this.Database.SaveChangesAsync();

            Console.WriteLine("User deleted successfully");
        }

        private async Task ListTokensAsync(string username)
        {
            var dbuser = await this.Database.Users
                .Include(x => x.Tokens)
                .FirstOrDefaultAsync(x => x.Id == username);

            if (dbuser == null)
            {
                Console.WriteLine("Specified user does not exist");
                this.PrintUsage(UsageKind.TokenList);
                return;
            }

            Console.WriteLine("Authentication tokens associated with {0}:", dbuser.Id);
            foreach (var dbtoken in dbuser.Tokens)
                Console.WriteLine("  {0}", this.Tokens.EncodeToken(new AuthenticationToken(dbtoken.UserId, dbtoken.IssuedAt.Value, dbtoken.Guid)));
        }

        private async Task IssueTokenAsync(string username)
        {
            var dbuser = await this.Database.Users
                .FirstOrDefaultAsync(x => x.Id == username);

            if (dbuser == null)
            {
                Console.WriteLine("Specified user does not exist");
                this.PrintUsage(UsageKind.TokenIssue);
                return;
            }

            var token = AuthenticationToken.IssueNew(dbuser.Id);

            await this.Database.Tokens.AddAsync(new Token
            {
                UserId = token.UserId,
                IssuedAt = token.IssuedAt.UtcDateTime,
                Guid = token.Guid
            });
            await this.Database.SaveChangesAsync();

            Console.WriteLine("Token issued");
            Console.WriteLine(this.Tokens.EncodeToken(token));
        }

        private async Task RevokeTokenAsync(string token)
        {
            if (!this.Tokens.TryReadTokenId(token, out var guid))
            {
                Console.WriteLine("Invalid token supplied");
                this.PrintUsage(UsageKind.TokenRevoke);
                return;
            }

            var dbtoken = await this.Database.Tokens
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Guid == guid);

            if (dbtoken == null)
            {
                Console.WriteLine("Specified token does not exist, possibly already revoked");
                this.PrintUsage(UsageKind.TokenList);
                return;
            }

            this.Database.Tokens.Remove(dbtoken);
            await this.Database.SaveChangesAsync();

            Console.WriteLine("Token revoked");
        }

        private void PrintUsage(UsageKind usageKind)
        {
            var prog = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

            switch (usageKind)
            {
                case UsageKind.User:
                    Console.WriteLine("Usage: dotnet {0} user <operation> [arguments...]", prog);
                    Console.WriteLine();
                    Console.WriteLine("Available operations on users:");
                    Console.WriteLine("  list           Lists all existing users");
                    Console.WriteLine("  create         Creates a new user");
                    Console.WriteLine("  delete         Deletes a user and all of their associated authentication tokens");
                    break;

                case UsageKind.Token:
                    Console.WriteLine("Usage: dotnet {0} token <operation> [arguments...]", prog);
                    Console.WriteLine();
                    Console.WriteLine("Available operations on tokens:");
                    Console.WriteLine("  list           Lists authentication tokens associated with a given user");
                    Console.WriteLine("  issue          Issues a new authentcation token for an existing user");
                    Console.WriteLine("  revoke         Revokes a previously-issued authentication token");
                    break;

                case UsageKind.UserList:
                    Console.WriteLine("Usage: dotnet {0} user list", prog);
                    break;

                case UsageKind.UserCreate:
                    Console.WriteLine("Usage: dotnet {0} user create <username> <email address>", prog);
                    Console.WriteLine();
                    Console.WriteLine("Arguments:");
                    Console.WriteLine("  username       Username of the new user; can only consist of a-z A-Z 0-9 - and _");
                    Console.WriteLine("  email address  Email address of the new user; must be valid");
                    break;

                case UsageKind.UserDelete:
                    Console.WriteLine("Usage: dotnet {0} user delete <username>", prog);
                    Console.WriteLine();
                    Console.WriteLine("Arguments:");
                    Console.WriteLine("  username       Username of the user to delete");
                    break;

                case UsageKind.TokenList:
                    Console.WriteLine("Usage: dotnet {0} token list <username>", prog);
                    Console.WriteLine();
                    Console.WriteLine("Arguments:");
                    Console.WriteLine("  username       Username of the user whose authentication tokens to list");
                    break;

                case UsageKind.TokenIssue:
                    Console.WriteLine("Usage: dotnet {0} token issue <username>", prog);
                    Console.WriteLine();
                    Console.WriteLine("Arguments:");
                    Console.WriteLine("  username       Username of the user for whom to issue an authentication token");
                    break;

                case UsageKind.TokenRevoke:
                    Console.WriteLine("Usage: dotnet {0} token revoke <token>", prog);
                    Console.WriteLine();
                    Console.WriteLine("Arguments:");
                    Console.WriteLine("  token          Authentication token to revoke");
                    break;

                case UsageKind.General:
                default:
                    Console.WriteLine("Usage: dotnet {0} <entity> <operation> [arguments...]", prog);
                    Console.WriteLine();
                    Console.WriteLine("Available entities:");
                    Console.WriteLine("  user           Provides ability to manage users");
                    Console.WriteLine("  token          Provides ability to manage authentication tokens");
                    Console.WriteLine();
                    Console.WriteLine("Available operations on users:");
                    Console.WriteLine("  list           Lists all existing users");
                    Console.WriteLine("  create         Creates a new user");
                    Console.WriteLine("  delete         Deletes a user and all of their associated authentication tokens");
                    Console.WriteLine();
                    Console.WriteLine("Available operations on tokens:");
                    Console.WriteLine("  list           Lists authentication tokens associated with a given user");
                    Console.WriteLine("  issue          Issues a new authentcation token for an existing user");
                    Console.WriteLine("  revoke         Revokes a previously-issued authentication token");
                    break;
            }
        }

        private static bool IsValidUsernameCharacter(char c)
            => c.IsBasicAlphanumeric() || c == '-' || c == '_';

        private enum UsageKind
        {
            General,
            User,
            UserList,
            UserCreate,
            UserDelete,
            Token,
            TokenList,
            TokenIssue,
            TokenRevoke
        }
    }
}
