using System.CommandLine;
using System.Reflection;
using LPS.Infrastructure.Watchdog;
using LPS.Domain.Domain.Common.Enums;
using LPS.Domain.Common.Interfaces;

namespace LPS.UI.Core.LPSCommandLine
{
    public static class CommandLineOptions
    {
        static CommandLineOptions()
        {
            // No changes needed here
        }

        // Helper method to add case variations
        private static void AddCaseInsensitiveAliases(Option option, params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                option.AddAlias(alias);
                var lowerAlias = alias.ToLowerInvariant();
                if (alias != lowerAlias)
                {
                    option.AddAlias(lowerAlias);
                }
                var upperAlias = alias.ToUpperInvariant();
                if (alias != upperAlias)
                {
                    option.AddAlias(upperAlias);
                }
                if (alias.StartsWith("--") && alias.Length > 2)
                {
                    var name = alias.Substring(2);
                    var camelCaseName = char.ToLowerInvariant(name[0]) + name.Substring(1);
                    var camelCaseAlias = "--" + camelCaseName;
                    if (alias != camelCaseAlias)
                    {
                        option.AddAlias(camelCaseAlias);
                    }
                }
            }
        }

        public static class LPSCommandOptions
        {
            static LPSCommandOptions()
            {
                // Shortcut aliases
                PlanNameOption.AddAlias("-n");
                RoundNameOption.AddAlias("-rn");
                StartupDelayOption.AddAlias("-sd");
                DelayClientCreationOption.AddAlias("-dcc");
                NumberOfClientsOption.AddAlias("-nc");
                ArrivalDelayOption.AddAlias("-ad");
                RunInParallelOption.AddAlias("-rip");
                SaveOption.AddAlias("-s");
                IterationNameOption.AddAlias("-in");
                RequestCountOption.AddAlias("-rc");
                Duration.AddAlias("-d");
                BatchSize.AddAlias("-bs");
                CoolDownTime.AddAlias("-cdt");
                HttpMethodOption.AddAlias("-hm");
                HttpVersionOption.AddAlias("-hv");
                UrlOption.AddAlias("-u");
                HeaderOption.AddAlias("-h");
                PayloadOption.AddAlias("-p");
                IterationModeOption.AddAlias("-im");
                MaximizeThroughputOption.AddAlias("-mt");
                DownloadHtmlEmbeddedResources.AddAlias("-dhtmler");
                SaveResponse.AddAlias("-sr");
                SupportH2C.AddAlias("-sh2c");

                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(PlanNameOption, "--name");
                AddCaseInsensitiveAliases(RoundNameOption, "--roundname");
                AddCaseInsensitiveAliases(StartupDelayOption, "--startupdelay");
                AddCaseInsensitiveAliases(DelayClientCreationOption, "--delayclientcreation");
                AddCaseInsensitiveAliases(NumberOfClientsOption, "--numberofclients");
                AddCaseInsensitiveAliases(ArrivalDelayOption, "--arrivaldelay");
                AddCaseInsensitiveAliases(RunInParallelOption, "--runinparallel");
                AddCaseInsensitiveAliases(SaveOption, "--save");
                AddCaseInsensitiveAliases(IterationNameOption, "--iterationname");
                AddCaseInsensitiveAliases(RequestCountOption, "--requestcount");
                AddCaseInsensitiveAliases(Duration, "--duration");
                AddCaseInsensitiveAliases(BatchSize, "--batchsize");
                AddCaseInsensitiveAliases(CoolDownTime, "--cooldowntime");
                AddCaseInsensitiveAliases(HttpMethodOption, "--method", "--httpmethod");
                AddCaseInsensitiveAliases(HttpVersionOption, "--httpversion");
                AddCaseInsensitiveAliases(UrlOption, "--url");
                AddCaseInsensitiveAliases(HeaderOption, "--header");
                AddCaseInsensitiveAliases(PayloadOption, "--payload");
                AddCaseInsensitiveAliases(IterationModeOption, "--iterationmode");
                AddCaseInsensitiveAliases(MaximizeThroughputOption, "--maximizethroughput");
                AddCaseInsensitiveAliases(DownloadHtmlEmbeddedResources, "--downloadhtmlembeddedresources");
                AddCaseInsensitiveAliases(SaveResponse, "--saveresponse");
                AddCaseInsensitiveAliases(SupportH2C, "--supporth2c");
            }


            public static Option<string> PlanNameOption { get; } = new Option<string>(
                "--name", () => "Quick-Test-Plan", "Plan name")
            {
                IsRequired = false,
                Arity = ArgumentArity.ExactlyOne
            };

            public static Option<string> RoundNameOption { get; } = new Option<string>(
                "--roundname", () => "Quick-Test-Round", "Round name")
            {
                IsRequired = false,
                Arity = ArgumentArity.ExactlyOne
            };

            public static Option<string> StartupDelayOption { get; } = new Option<string>(
                "--startupDelay", "Add startup (in seconds) delay to your round")
            {
                IsRequired = false
            };
            public static Option<string> NumberOfClientsOption { get; } = new Option<string>(
                "--numberofclients", () => "1", "Number of clients to perform the test round")
            {
                IsRequired = false
            };

            public static Option<string?> ArrivalDelayOption { get; } = new Option<string?>(
                "--arrivaldelay", "Time in milliseconds to wait before a new client arrives")
            {
                IsRequired = false
            };

            public static Option<string> DelayClientCreationOption { get; } = new Option<string>(
                name:"--delayclientcreation",
                description:"Delay client creation until needed",
                parseArgument: ParseBoolOptionArgument)
            {
                IsRequired = false,
                Arity = ArgumentArity.ZeroOrOne // Allows zero or one argument
            };

            public static Option<string> RunInParallelOption { get; } = new Option<string>(
                name:"--runinparallel", 
                description:"Execute your iterations in parallel",
                parseArgument: ParseBoolOptionArgument)
            {
                IsRequired = false,
                Arity = ArgumentArity.ZeroOrOne // Allows zero or one argument
            };

            public static Option<bool> SaveOption { get; } = new Option<bool>(
                "--save", () => false, "Save the test as yaml/json file to disk")
            {
                IsRequired = false
            };

            public static Option<string> UrlOption { get; } = new Option<string>(
                "--url", "URL")
            {
                IsRequired = true
            };

            public static Option<string> HttpMethodOption { get; } = new Option<string>(
                "--method", () => "GET", "HTTP method")
            {
                IsRequired = false
            };

            public static Option<IList<string>> HeaderOption { get; } = new Option<IList<string>>(
                "--header", "Header")
            {
                IsRequired = false,
                AllowMultipleArgumentsPerToken = true
            };

            public static Option<string> IterationNameOption { get; } = new Option<string>(
                "--iterationname", () => "Quick-Http-Iteration", "Iteration name")
            {
                IsRequired = false
            };

            public static Option<string> IterationModeOption { get; } = new Option<string>(
                "--iterationmode", () => IterationMode.R.ToString(), "Defines iteration mode")
            {
                IsRequired = false
            };

            public static Option<string> MaximizeThroughputOption { get; } = new Option<string>(
                name:"--maximizethroughput",
                description: "Maximize test throughput. Maximizing test throughput may lead to significantly higher CPU and memory usage.",
                parseArgument: ParseBoolOptionArgument)
            {
                IsRequired = false,
                Arity = ArgumentArity.ZeroOrOne // Allows zero or one argument
            };

            public static Option<string?> RequestCountOption { get; } = new Option<string?>(
                "--requestcount", "Number of requests")
            {
                IsRequired = false
            };

            public static Option<string?> Duration { get; } = new Option<string?>(
                "--duration", "Duration in seconds")
            {
                IsRequired = false
            };

            public static Option<string?> CoolDownTime { get; } = new Option<string?>(
                "--cooldowntime", "Cooldown time in milliseconds")
            {
                IsRequired = false
            };

            public static Option<string?> BatchSize { get; } = new Option<string?>(
                "--batchsize", "Batch size")
            {
                IsRequired = false
            };

            public static Option<string> HttpVersionOption { get; } = new Option<string>(
                "--httpversion", () => "2.0", "HTTP version")
            {
                IsRequired = false
            };

            public static Option<string> DownloadHtmlEmbeddedResources { get; } = new Option<string>(
                name: "--downloadhtmlembeddedresources",
                description: "Download HTML embedded resources",
                parseArgument: ParseBoolOptionArgument)
            {
                IsRequired = false,
                Arity = ArgumentArity.ZeroOrOne // Allows zero or one argument
            };

            public static Option<string> SaveResponse { get; } = new Option<string>(
                name: "--saveresponse",
                description: "Save HTTP response",
                parseArgument: ParseBoolOptionArgument)
            {
                IsRequired = false,
                Arity = ArgumentArity.ZeroOrOne // Allows zero or one argument
            };
            public static Option<string?> SupportH2C { get; } = new Option<string?>(
                name: "--supporth2c", 
                description: "Enables support for HTTP/2 over clear text. If used with a non-HTTP/2 protocol, it will override the protocol setting and enforce HTTP/2.", 
                parseArgument: ParseBoolOptionArgument)
            {
                IsRequired = false,
                Arity = ArgumentArity.ZeroOrOne // Allows zero or one argument
            };

            public static Option<string> PayloadOption { get; } = new Option<string>(
                "--payload", () => string.Empty, "Request payload")
            {
                IsRequired = false
            };
        }

        public static class LPSRunCommandOptions
        {
            static LPSRunCommandOptions()
            {
                RoundNameOption.AddAlias("-rn");
                AddCaseInsensitiveAliases(RoundNameOption, "--roundname");
                TagOption.AddAlias("-t");
                AddCaseInsensitiveAliases(TagOption, "--tag");

                EnvironmentOption.AddAlias("-e");
                AddCaseInsensitiveAliases(EnvironmentOption, "--environment");
            }
            public static Option<IList<string>> RoundNameOption { get; } = new Option<IList<string>>(
            "--roundname", "Round name")
            {
                IsRequired = false,
                AllowMultipleArgumentsPerToken = true
            };

            public static Option<IList<string>> TagOption { get; } = new Option<IList<string>>(
                "--tag", "tag(s)")
            {
                IsRequired = false,
                AllowMultipleArgumentsPerToken = true
            };

            public static Option<IList<string>> EnvironmentOption { get; } = new Option<IList<string>>(
            "--environment", "Run a against a specifiv environment(s)")
            {
                IsRequired = false,
                AllowMultipleArgumentsPerToken = true
            };
            public static Argument<string> ConfigFileArgument { get; } = new Argument<string>(
                "config", // This makes it positional
                "Test configuration file name"
            )
            {
                Arity = ArgumentArity.ExactlyOne
            };
        }

        public static class LPSCreateCommandOptions
        {
            static LPSCreateCommandOptions()
            {

            }
            public static Argument<string> ConfigFileArgument { get; } = new Argument<string>(
                "config", // This makes it positional
                "Test configuration file name"
            )
            {
                Arity = ArgumentArity.ExactlyOne
            };


            public static Option<string> PlanNameOption { get; } = new Option<string>(
                "--name", "Plan name")
            {
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne
            };
        }

        public static class LPSRoundCommandOptions
        {
            static LPSRoundCommandOptions()
            {
                // Shortcut aliases
                RoundNameOption.AddAlias("-n");
                BaseUrlOption.AddAlias("-burl");
                StartupDelayOption.AddAlias("-sd");
                DelayClientCreation.AddAlias("-dcc");
                NumberOfClientsOption.AddAlias("-nc");
                ArrivalDelayOption.AddAlias("-ad");
                RunInParallel.AddAlias("-rip");
                TagOption.AddAlias("-t");

                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(RoundNameOption, "--name");
                AddCaseInsensitiveAliases(BaseUrlOption, "--baseurl");
                AddCaseInsensitiveAliases(StartupDelayOption, "--startupdelay");
                AddCaseInsensitiveAliases(DelayClientCreation, "--delayclientcreation");
                AddCaseInsensitiveAliases(NumberOfClientsOption, "--numberofclients");
                AddCaseInsensitiveAliases(ArrivalDelayOption, "--arrivaldelay");
                AddCaseInsensitiveAliases(RunInParallel, "--runinparallel");
                AddCaseInsensitiveAliases(TagOption, "--tag");
            }
            public static Argument<string> ConfigFileArgument { get; } = new Argument<string>(
                "config", // This makes it positional
                "Test configuration file name"
            )
            {
                Arity = ArgumentArity.ExactlyOne
            };


            public static Option<string> RoundNameOption { get; } = new Option<string>(
                "--name", "Round name")
            {
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne
            };

            public static Option<string> BaseUrlOption { get; } = new Option<string>(
            "--baseUrl", "Base URL of the target endpostring")
            {
                IsRequired = false,
                Arity = ArgumentArity.ExactlyOne
            };

            public static Option<string> StartupDelayOption { get; } = new Option<string>(
                "--startupDelay", "Add startup (in seconds) delay to your round")
            {
                IsRequired = false
            };

            public static Option<string> NumberOfClientsOption { get; } = new Option<string>(
                "--numberofclients", () => 1.ToString(), "Number of clients to perform the test round")
            {
                IsRequired = true
            };

            public static Option<string> ArrivalDelayOption { get; } = new Option<string>(
                "--arrivaldelay", "Time in milliseconds to wait before a new client arrives. It must be greater than 0 when the number of clients is more than 1")
            {
                IsRequired = false
            };

            public static Option<string> DelayClientCreation { get; } = new Option<string>(
               name: "--delayclientcreation", 
               description:"Delay client creation until needed",
                parseArgument: ParseBoolOptionArgument)
            {
                IsRequired = false,
                Arity = ArgumentArity.ZeroOrOne // Allows zero or one argument
            };

            public static Option<string?> RunInParallel { get; } = new Option<string?>(
                name:"--runinparallel", 
                description:"Execute your iterations in parallel",
                parseArgument: ParseBoolOptionArgument)
            {
                IsRequired = false,
                Arity = ArgumentArity.ZeroOrOne // Allows zero or one argument
            };

            public static Option<IList<string>> TagOption { get; } = new Option<IList<string>>(
                    "--tag", "tag")
            {
                IsRequired = false,
                AllowMultipleArgumentsPerToken = true
            };
        }

        public static class RefCommandOptions
        {
            static RefCommandOptions()
            {
                // Shortcut aliases
                RoundNameOption.AddAlias("-rn");
                IterationNameOption.AddAlias("-n");
                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(RoundNameOption, "--roundName");
                AddCaseInsensitiveAliases(IterationNameOption, "--name");
            }

            public static Argument<string> ConfigFileArgument { get; } = new Argument<string>(
                "config", // This makes it positional
                "Test configuration file name")
            {
                Arity = ArgumentArity.ExactlyOne
            };

            public static Option<string> RoundNameOption { get; } = new Option<string>(
                "--roundname", "Round name")
            {
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne
            };

            public static Option<string> IterationNameOption { get; } = new Option<string>(
                "--name", "Iteration name")
            {
                IsRequired = true
            };
        }

        public static class VariableCommandOptions
        {
            static VariableCommandOptions()
            {
                // Shortcut aliases
                NameOption.AddAlias("-n");
                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(NameOption, "--name");

                // Shortcut aliases
                ValueOption.AddAlias("-v");
                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(ValueOption, "--value");

                // Shortcut aliases
                AsOption.AddAlias("-as");
                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(AsOption, "--as");


                // Shortcut aliases
                RegexOption.AddAlias("-r");
                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(RegexOption, "--regex");

                // Shortcut aliases
                EnvironmentOption.AddAlias("-e");
                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(EnvironmentOption, "--environment");
            }

            public static Argument<string> ConfigFileArgument { get; } = new Argument<string>(
                "config", // This makes it positional
                "Test configuration file name")
            {
                Arity = ArgumentArity.ExactlyOne
            };

            public static Option<string> NameOption { get; } = new Option<string>(
                "--name", "Variable name")
            {
                IsRequired = true
            };

            public static Option<IList<string>> EnvironmentOption { get; } = new Option<IList<string>>(
            "--environment", "Run a against a specifiv environment(s)")
            {
                IsRequired = false,
                AllowMultipleArgumentsPerToken = true
            };

            public static Option<string> ValueOption { get; } = new Option<string>(
                "--value", "Variable value")
            {
                IsRequired = true
            };

            public static Option<string> AsOption { get; } = new Option<string>(
                "--as", () => string.Empty, "Read the variable As (Text, Json or XML)")
            {
                IsRequired = false
            };

            public static Option<string> RegexOption { get; } = new Option<string>(
                "--regex", () => string.Empty, "regex to apply at your value")
            {
                IsRequired = false
            };
        }

        public static class CaptureCommandOptions
        {
            static CaptureCommandOptions()
            {
                IterationNameOption.AddAlias("-in");
                AddCaseInsensitiveAliases(IterationNameOption, "--iterationname");
                RoundNameOption.AddAlias("-rn");
                AddCaseInsensitiveAliases(RoundNameOption, "--roundname");


                // Shortcut aliases
                ToOption.AddAlias("-to");
                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(ToOption, "--to");
                // Shortcut aliases
                AsOption.AddAlias("-as");
                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(AsOption, "--as");


                // Shortcut aliases
                RegexOption.AddAlias("-r");
                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(RegexOption, "--regex");

                MakeGlobal.AddAlias("-mg");
                AddCaseInsensitiveAliases(MakeGlobal, "--makeglobal");


                HeaderOption.AddAlias("-h");
                AddCaseInsensitiveAliases(HeaderOption, "--header");
            }

            public static Option<string> IterationNameOption { get; } = new Option<string>(
                "--iterationname", "Iteration name")
            {
                IsRequired = true
            };
            public static Option<string> RoundNameOption { get; } = new Option<string>(
                "--roundName", "Round name")
            {
                IsRequired = false
            };
            public static Argument<string> ConfigFileArgument { get; } = new Argument<string>(
                "config", // This makes it positional
                "Test configuration file name")
            {
                Arity = ArgumentArity.ExactlyOne
            };


            public static Option<string> ToOption { get; } = new Option<string>(
                "--to", "Variable name")
            {
                IsRequired = true
            };

            public static Option<IList<string>> HeaderOption { get; } = new Option<IList<string>>(
            "--header", "Header")
            {
                IsRequired = false,
                AllowMultipleArgumentsPerToken = true
            };

            public static Option<string> AsOption { get; } = new Option<string>(
                "--as", () => string.Empty, "Read the variable As (Text, Json or XML)")
            {
                IsRequired = false
            };


            public static Option<string> MakeGlobal { get; } = new Option<string>(
                name:"--makeGlobal", 
                description: "Store the response as a global variable",
                parseArgument: ParseBoolOptionArgument)
            {
                IsRequired = false,
                Arity = ArgumentArity.ZeroOrOne // Allows zero or one argument
            };

            public static Option<string> RegexOption { get; } = new Option<string>(
                "--regex", () => string.Empty, "regex to apply at your value")
            {
                IsRequired = false
            };

        }
        public static class LPSIterationCommandOptions
        {
            static LPSIterationCommandOptions()
            {
                // Shortcut aliases
                StartupDelayOption.AddAlias("-sd");
                RoundNameOption.AddAlias("-rn");
                IterationNameOption.AddAlias("-n");
                GlobalOption.AddAlias("-g");
                RequestCountOption.AddAlias("-rc");
                Duration.AddAlias("-d");
                BatchSize.AddAlias("-bs");
                CoolDownTime.AddAlias("-cdt");
                HttpMethodOption.AddAlias("-hm");
                HttpVersionOption.AddAlias("-hv");
                UrlOption.AddAlias("-u");
                HeaderOption.AddAlias("-h");
                PayloadOption.AddAlias("-p");
                IterationModeOption.AddAlias("-im");
                MaximizeThroughputOption.AddAlias("-mt");
                DownloadHtmlEmbeddedResources.AddAlias("-dhtmler");
                SaveResponse.AddAlias("-sr");
                SupportH2C.AddAlias("-h2c");

                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(RoundNameOption, "--roundName");
                AddCaseInsensitiveAliases(IterationNameOption, "--name");
                AddCaseInsensitiveAliases(StartupDelayOption, "--startupdelay");
                AddCaseInsensitiveAliases(GlobalOption, "--global");
                AddCaseInsensitiveAliases(RequestCountOption, "--requestcount");
                AddCaseInsensitiveAliases(Duration, "--duration");
                AddCaseInsensitiveAliases(BatchSize, "--batchsize");
                AddCaseInsensitiveAliases(CoolDownTime, "--cooldowntime");
                AddCaseInsensitiveAliases(HttpMethodOption, "--method", "--httpmethod");
                AddCaseInsensitiveAliases(HttpVersionOption, "--httpversion");
                AddCaseInsensitiveAliases(UrlOption, "--url");
                AddCaseInsensitiveAliases(HeaderOption, "--header");
                AddCaseInsensitiveAliases(PayloadOption, "--payload");
                AddCaseInsensitiveAliases(IterationModeOption, "--iterationmode");
                AddCaseInsensitiveAliases(MaximizeThroughputOption, "--maximizethroughput");
                AddCaseInsensitiveAliases(DownloadHtmlEmbeddedResources, "--downloadhtmlembeddedresources");
                AddCaseInsensitiveAliases(SaveResponse, "--saveresponse");
                AddCaseInsensitiveAliases(SupportH2C, "--supporth2c");
            }
            public static Argument<string> ConfigFileArgument { get; } = new Argument<string>(
                "config", // This makes it positional
                "Test configuration file name"
            )
            {
                Arity = ArgumentArity.ExactlyOne
            };
            public static Option<string> RoundNameOption { get; } = new Option<string>(
                "--roundname", "Round name")
            {
                IsRequired = false,
                Arity = ArgumentArity.ExactlyOne
            };
            public static Option<string> IterationNameOption { get; } = new Option<string>(
                "--name", "Iteration name")
            {
                IsRequired = true
            };
            public static Option<string> IterationModeOption { get; } = new Option<string>(
                "--iterationMode", "Defines iteration mode")
            {
                IsRequired = true
            };
            public static Option<string> StartupDelayOption { get; } = new Option<string>(
                "--startupdelay", "Add startup delay to your round")
            {
                IsRequired = false
            };
            public static Option<string> MaximizeThroughputOption { get; } = new Option<string>(
                name:"--maximizethroughput", 
                description: "Maximize test throughput. Maximizing test throughput may lead to significantly higher CPU and memory usage.",
                parseArgument: ParseBoolOptionArgument)
            {
                IsRequired = false,
                Arity = ArgumentArity.ZeroOrOne // Allows zero or one argument
            };
            public static Option<string?> RequestCountOption { get; } = new Option<string?>(
                "--requestcount", "Number of requests")
            {
                IsRequired = false
            };
            public static Option<string?> Duration { get; } = new Option<string?>(
                "--duration", "Duration in seconds")
            {
                IsRequired = false
            };
            public static Option<string?> CoolDownTime { get; } = new Option<string?>(
                "--cooldowntime", "Cooldown time in milliseconds")
            {
                IsRequired = false
            };
            public static Option<string?> BatchSize { get; } = new Option<string?>(
                "--batchsize", "Batch size")
            {
                IsRequired = false
            };
            public static Option<string> HttpMethodOption { get; } = new Option<string>(
                "--method", "HTTP method")
            {
                IsRequired = true
            };
            public static Option<string> HttpVersionOption { get; } = new Option<string>(
                "--httpversion", () => "2.0", "HTTP version")
            {
                IsRequired = false
            };
            public static Option<string> UrlOption { get; } = new Option<string>(
                "--url", "URL")
            {
                IsRequired = true
            };
            public static Option<string> DownloadHtmlEmbeddedResources { get; } = new Option<string>(
                name: "--downloadhtmlembeddedresources", 
                description: "Download HTML embedded resources",
                parseArgument: ParseBoolOptionArgument)
            {
                IsRequired = false,
                Arity = ArgumentArity.ZeroOrOne // Allows zero or one argument
            };
            public static Option<string> SaveResponse { get; } = new Option<string>(
                name: "--saveresponse", 
                description: "Save HTTP response",
                parseArgument: ParseBoolOptionArgument)
            {
                IsRequired = false,
                Arity = ArgumentArity.ZeroOrOne // Allows zero or one argument
            };
            public static Option<string?> SupportH2C { get; } = new Option<string?>(
                name: "--supporth2c", 
                description: "Enables support for HTTP/2 over clear text. If used with a non-HTTP/2 protocol, it will override the protocol setting and enforce HTTP/2.",
                parseArgument: ParseBoolOptionArgument)
            {
                IsRequired = false,
                Arity = ArgumentArity.ZeroOrOne // Allows zero or one argument
            };
            public static Option<IList<string>> HeaderOption { get; } = new Option<IList<string>>(
                "--header", "Header")
            {
                IsRequired = false,
                AllowMultipleArgumentsPerToken = true
            };
            public static Option<string> PayloadOption { get; } = new Option<string>(
                "--payload", "Request payload")
            {
                IsRequired = false
            };
            public static Option<bool> GlobalOption { get; } = new Option<bool>(
                "--global", () => false, "Save as a global iteration")
            {
                IsRequired = false
            };
        }

        public static class LPSLoggerCommandOptions
        {
            static LPSLoggerCommandOptions()
            {
                // Shortcut aliases
                LogFilePathOption.AddAlias("-lfp");
                DisableFileLoggingOption.AddAlias("-dfl");
                EnableConsoleLoggingOption.AddAlias("-ecl");
                DisableConsoleErrorLoggingOption.AddAlias("-dcel");
                LoggingLevelOption.AddAlias("-ll");
                ConsoleLoggingLevelOption.AddAlias("-cll");

                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(LogFilePathOption, "--logfilepath");
                AddCaseInsensitiveAliases(DisableFileLoggingOption, "--disablefilelogging");
                AddCaseInsensitiveAliases(EnableConsoleLoggingOption, "--enableconsolelogging");
                AddCaseInsensitiveAliases(DisableConsoleErrorLoggingOption, "--disableconsoleerrorlogging");
                AddCaseInsensitiveAliases(LoggingLevelOption, "--logginglevel");
                AddCaseInsensitiveAliases(ConsoleLoggingLevelOption, "--consolelogginglevel");
            }

            public static Option<string> LogFilePathOption { get; } = new Option<string>(
                "--logfilepath", "Path to log file")
            {
                IsRequired = false
            };

            public static Option<bool?> EnableConsoleLoggingOption { get; } = new Option<bool?>(
                "--enableconsolelogging", "Enable console logging")
            {
                IsRequired = false
            };

            public static Option<bool?> DisableConsoleErrorLoggingOption { get; } = new Option<bool?>(
                "--disableconsoleerrorlogging", "Disable console error logging")
            {
                IsRequired = false
            };

            public static Option<bool?> DisableFileLoggingOption { get; } = new Option<bool?>(
                "--disablefilelogging", "Disable file logging")
            {
                IsRequired = false
            };

            public static Option<LPSLoggingLevel?> LoggingLevelOption { get; } = new Option<LPSLoggingLevel?>(
                "--logginglevel", "Logging level")
            {
                IsRequired = false
            };

            public static Option<LPSLoggingLevel?> ConsoleLoggingLevelOption { get; } = new Option<LPSLoggingLevel?>(
                "--consolelogginglevel", "Console logging level")
            {
                IsRequired = false
            };
        }

        public static class LPSHttpClientCommandOptions
        {
            static LPSHttpClientCommandOptions()
            {
                // Shortcut aliases
                MaxConnectionsPerServerOption.AddAlias("-mcps");
                PoolConnectionLifetimeOption.AddAlias("-pclt");
                PoolConnectionIdleTimeoutOption.AddAlias("-pcit");
                ClientTimeoutOption.AddAlias("-cto");

                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(MaxConnectionsPerServerOption, "--maxconnectionsperserver");
                AddCaseInsensitiveAliases(PoolConnectionLifetimeOption, "--poolconnectionlifetime");
                AddCaseInsensitiveAliases(PoolConnectionIdleTimeoutOption, "--poolconnectionidletimeout");
                AddCaseInsensitiveAliases(ClientTimeoutOption, "--clienttimeout");
            }

            public static Option<int?> MaxConnectionsPerServerOption { get; } = new Option<int?>(
                "--maxconnectionsperserver", "Max connections per server")
            {
                IsRequired = false
            };

            public static Option<int?> PoolConnectionLifetimeOption { get; } = new Option<int?>(
                "--poolconnectionlifetime", "Pooled connection lifetime in seconds")
            {
                IsRequired = false
            };

            public static Option<int?> PoolConnectionIdleTimeoutOption { get; } = new Option<int?>(
                "--poolconnectionidletimeout", "Pooled connection idle timeout")
            {
                IsRequired = false
            };

            public static Option<int?> ClientTimeoutOption { get; } = new Option<int?>(
                "--clienttimeout", "Client timeout in seconds")
            {
                IsRequired = false
            };
        }

        public static class LPSWatchdogCommandOptions
        {
            static LPSWatchdogCommandOptions()
            {
                // Shortcut aliases
                MaxMemoryMB.AddAlias("-mmm");
                MaxCPUPercentage.AddAlias("-mcp");
                CoolDownMemoryMB.AddAlias("-cdmm");
                CoolDownCPUPercentage.AddAlias("-cdcp");
                MaxConcurrentConnectionsCountPerHostName.AddAlias("-mcccphn");
                CoolDownConcurrentConnectionsCountPerHostName.AddAlias("-cdcccphn");
                MaxCoolingPeriod.AddAlias("-maxcp");
                ResumeCoolingAfter.AddAlias("-rca");
                CoolDownRetryTimeInSeconds.AddAlias("-cdrtis");
                SuspensionMode.AddAlias("-sm");

                // Add case-insensitive aliases
                AddCaseInsensitiveAliases(MaxMemoryMB, "--maxmemorymb");
                AddCaseInsensitiveAliases(MaxCPUPercentage, "--maxcpupercentage");
                AddCaseInsensitiveAliases(CoolDownMemoryMB, "--cooldownmemorymb");
                AddCaseInsensitiveAliases(CoolDownCPUPercentage, "--cooldowncpupercentage");
                AddCaseInsensitiveAliases(MaxConcurrentConnectionsCountPerHostName, "--maxconcurrentconnectionscountperhostname");
                AddCaseInsensitiveAliases(CoolDownConcurrentConnectionsCountPerHostName, "--cooldownconcurrentconnectionscountperhostname");
                AddCaseInsensitiveAliases(CoolDownRetryTimeInSeconds, "--cooldownretrytimeinseconds");
                AddCaseInsensitiveAliases(MaxCoolingPeriod, "--maxcoolingperiod");
                AddCaseInsensitiveAliases(ResumeCoolingAfter, "--resumecoolingafter");
                AddCaseInsensitiveAliases(SuspensionMode, "--suspensionmode");
            }

            public static Option<int?> MaxMemoryMB { get; } = new Option<int?>(
                "--maxmemorymb", "Memory threshold in MB")
            {
                IsRequired = false
            };

            public static Option<int?> MaxCPUPercentage { get; } = new Option<int?>(
                "--maxcpupercentage", "CPU threshold percentage")
            {
                IsRequired = false
            };

            public static Option<int?> CoolDownMemoryMB { get; } = new Option<int?>(
                "--cooldownmemorymb", "Memory cooldown in MB")
            {
                IsRequired = false
            };

            public static Option<int?> CoolDownCPUPercentage { get; } = new Option<int?>(
                "--cooldowncpupercentage", "CPU cooldown percentage")
            {
                IsRequired = false
            };

            public static Option<int?> MaxConcurrentConnectionsCountPerHostName { get; } = new Option<int?>(
                "--maxconcurrentconnectionscountperhostname", "Max concurrent connections per hostname")
            {
                IsRequired = false
            };

            public static Option<int?> CoolDownConcurrentConnectionsCountPerHostName { get; } = new Option<int?>(
                "--cooldownconcurrentconnectionscountperhostname", "Cooldown concurrent connections per hostname")
            {
                IsRequired = false
            };

            public static Option<int?> CoolDownRetryTimeInSeconds { get; } = new Option<int?>(
                "--cooldownretrytimeinseconds", "Cooldown retry stringerval in seconds")
            {
                IsRequired = false
            };

            public static Option<int?> MaxCoolingPeriod { get; } = new Option<int?>(
                "--maxcoolingperiod", "Maximum cooling period in seconds")
            {
                IsRequired = false
            };

            public static Option<int?> ResumeCoolingAfter { get; } = new Option<int?>(
                "--resumecoolingafter", "Resume cooling after seconds")
            {
                IsRequired = false
            };

            public static Option<SuspensionMode?> SuspensionMode { get; } = new Option<SuspensionMode?>(
                "--suspensionmode", "Suspension approach ('All' or 'Any')")
            {
                IsRequired = false
            };
        }
        public static string ParseBoolOptionArgument(System.CommandLine.Parsing.ArgumentResult result)
        {
            if (result.Tokens.Count == 0)
            {
                return "true"; // Default to "false" when no value is provided
            }

            return result.Tokens[0].Value;

            throw new ArgumentException("Unexpected number of tokens for the option.");
        }

        public static void AddOptionsToCommand(Command command, Type optionsType)
        {
            var properties = optionsType.GetProperties(
                BindingFlags.Public | BindingFlags.Static);

            foreach (var property in properties)
            {
                if (property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition() ==
                    typeof(Option<>))
                {
                    var optionInstance = (Option)property.GetValue(null);
                    command.AddOption(optionInstance);
                }
            }
        }
    }
}
