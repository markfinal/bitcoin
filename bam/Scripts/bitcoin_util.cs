using Bam.Core;
using System.Linq;
namespace bitcoin
{
    sealed class bitcoin_util :
        C.StaticLibrary
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            this.Macros["libprefix"] = TokenizedString.CreateVerbatim("lib");

            var source = this.CreateCxxSourceContainer("$(packagedir)/src/support/pagelocker.cpp");
            source.AddFiles("$(packagedir)/src/chainparamsbase.cpp");
            source.AddFiles("$(packagedir)/src/clientversion.cpp");
            source.AddFiles("$(packagedir)/src/compat/glibc_sanity.cpp"); // TODO: needed for Windows?
            source.AddFiles("$(packagedir)/src/compat/glibcxx_sanity.cpp"); // TODO: needed for Windows?
            source.AddFiles("$(packagedir)/src/compat/strnlen.cpp"); // TODO: needed for Windows?
            source.AddFiles("$(packagedir)/src/random.cpp");
            source.AddFiles("$(packagedir)/src/rpcprotocol.cpp");
            source.AddFiles("$(packagedir)/src/support/cleanse.cpp");
            source.AddFiles("$(packagedir)/src/sync.cpp");
            source.AddFiles("$(packagedir)/src/uint256.cpp");
            source.AddFiles("$(packagedir)/src/util.cpp");
            source.AddFiles("$(packagedir)/src/utilmoneystr.cpp");
            source.AddFiles("$(packagedir)/src/utilstrencodings.cpp");
            source.AddFiles("$(packagedir)/src/utiltime.cpp");

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src"));
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src/univalue/include"));

                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        vcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level2;
                    }
                });

            // publicly, because the dependency pokes out of a header file, in src/support/pagelocker.h
            this.CompileAgainstPublicly<boost.Thread>(source);

            if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows))
            {
                source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.PreprocessorDefines.Add("WIN32");
                    compiler.PreprocessorDefines.Add("HAVE_DECL_STRNLEN", "1");

                    var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                    cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;
                });
                this.CompileAgainst<WindowsSDK.WindowsSDK>(source);
            }

            // this adds OpenSSL support to files that need it
            source.Children.Where(item => item.InputPath.Parse().EndsWith("random.cpp")).ToList().ForEach(item =>
                {
                    this.CompileAgainst<openssl.OpenSSL>(item);

                    // TODO: note that the above does not automatically add the procedural header generation as a dependency
                    // for the source here, so it is a manual process to add the dependencies
                    // Note: the SOURCE depends on these, as they are header generation modules

                    var openSSLCopyStandardHeaders = Graph.Instance.FindReferencedModule<openssl.CopyStandardHeaders>();
                    item.DependsOn(openSSLCopyStandardHeaders);
                });
            source.Children.Where(item => item.InputPath.Parse().EndsWith("cleanse.cpp")).ToList().ForEach(item =>
                {
                    this.CompileAgainst<openssl.OpenSSL>(item);

                    // TODO: note that the above does not automatically add the procedural header generation as a dependency
                    // for the source here, so it is a manual process to add the dependencies
                    // Note: the SOURCE depends on these, as they are header generation modules

                    var openSSLCopyStandardHeaders = Graph.Instance.FindReferencedModule<openssl.CopyStandardHeaders>();
                    item.DependsOn(openSSLCopyStandardHeaders);
                });
            source.Children.Where(item => item.InputPath.Parse().EndsWith("util.cpp")).ToList().ForEach(item =>
                {
                    this.CompileAgainst<openssl.OpenSSL>(item);

                    // TODO: note that the above does not automatically add the procedural header generation as a dependency
                    // for the source here, so it is a manual process to add the dependencies
                    // Note: the SOURCE depends on these, as they are header generation modules

                    var openSSLCopyStandardHeaders = Graph.Instance.FindReferencedModule<openssl.CopyStandardHeaders>();
                    item.DependsOn(openSSLCopyStandardHeaders);
                });

            // apparently, Boost had a broken sleep_for implementation
            source.Children.Where(item => item.InputPath.Parse().EndsWith("utiltime.cpp")).ToList().ForEach(item =>
                {
                    item.PrivatePatch(settings =>
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.PreprocessorDefines.Add("HAVE_WORKING_BOOST_SLEEP_FOR"); // assuming Boost 1.60.0 is working
                        });
                });
        }
    }
}
