using Bam.Core;
using System.Linq;
namespace bitcoin
{
    sealed class bitcoin_server :
        C.StaticLibrary
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            this.Macros["libprefix"] = TokenizedString.CreateVerbatim("lib");

            var source = this.CreateCxxSourceContainer("$(packagedir)/src/addrman.cpp");
            source.AddFiles("$(packagedir)/src/alert.cpp");
            source.AddFiles("$(packagedir)/src/bloom.cpp");
            source.AddFiles("$(packagedir)/src/chain.cpp");
            source.AddFiles("$(packagedir)/src/checkpoints.cpp");
            source.AddFiles("$(packagedir)/src/httprpc.cpp");
            source.AddFiles("$(packagedir)/src/httpserver.cpp");
            source.AddFiles("$(packagedir)/src/init.cpp");
            source.AddFiles("$(packagedir)/src/dbwrapper.cpp");
            source.AddFiles("$(packagedir)/src/main.cpp");
            source.AddFiles("$(packagedir)/src/merkleblock.cpp");
            source.AddFiles("$(packagedir)/src/miner.cpp");
            source.AddFiles("$(packagedir)/src/net.cpp");
            source.AddFiles("$(packagedir)/src/noui.cpp");
            source.AddFiles("$(packagedir)/src/policy/fees.cpp");
            source.AddFiles("$(packagedir)/src/policy/policy.cpp");
            source.AddFiles("$(packagedir)/src/pow.cpp");
            source.AddFiles("$(packagedir)/src/rest.cpp");
            source.AddFiles("$(packagedir)/src/rpcblockchain.cpp");
            source.AddFiles("$(packagedir)/src/rpcmining.cpp");
            source.AddFiles("$(packagedir)/src/rpcmisc.cpp");
            source.AddFiles("$(packagedir)/src/rpcnet.cpp");
            source.AddFiles("$(packagedir)/src/rpcrawtransaction.cpp");
            source.AddFiles("$(packagedir)/src/rpcserver.cpp");
            source.AddFiles("$(packagedir)/src/script/sigcache.cpp");
            source.AddFiles("$(packagedir)/src/timedata.cpp");
            source.AddFiles("$(packagedir)/src/torcontrol.cpp");
            source.AddFiles("$(packagedir)/src/txdb.cpp");
            source.AddFiles("$(packagedir)/src/txmempool.cpp");
            source.AddFiles("$(packagedir)/src/validationinterface.cpp");

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src"));
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src/univalue/include"));
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src/leveldb/include"));

                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        vcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level1;
                    }
                });

            // publicly, because the dependency pokes out of a header file, in src/sync.h
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

            // libevent dependency
            source.Children.Where(item => item.InputPath.Parse().EndsWith("httpserver.cpp")).ToList().ForEach(item =>
                {
                    this.CompileAgainst<libevent.libevent>(item);

                    // TODO: note that the above does not automatically add the procedural header generation as a dependency
                    // for the source here, so it is a manual process to add the dependencies
                    // Note: the SOURCE depends on these, as they are header generation modules

                    var libeventConfigHeaderGeneration = Graph.Instance.FindReferencedModule<libevent.GenerateConfigHeader>();
                    item.DependsOn(libeventConfigHeaderGeneration);
                });
            source.Children.Where(item => item.InputPath.Parse().EndsWith("torcontrol.cpp")).ToList().ForEach(item =>
            {
                this.CompileAgainst<libevent.libevent>(item);

                // TODO: note that the above does not automatically add the procedural header generation as a dependency
                // for the source here, so it is a manual process to add the dependencies
                // Note: the SOURCE depends on these, as they are header generation modules

                var libeventConfigHeaderGeneration = Graph.Instance.FindReferencedModule<libevent.GenerateConfigHeader>();
                item.DependsOn(libeventConfigHeaderGeneration);
            });

            // this adds OpenSSL support to files that need it
            source.Children.Where(item => item.InputPath.Parse().EndsWith("init.cpp")).ToList().ForEach(item =>
                {
                    this.CompileAgainst<openssl.OpenSSL>(item);

                    // TODO: note that the above does not automatically add the procedural header generation as a dependency
                    // for the source here, so it is a manual process to add the dependencies
                    // Note: the SOURCE depends on these, as they are header generation modules

                    var openSSLCopyStandardHeaders = Graph.Instance.FindReferencedModule<openssl.CopyStandardHeaders>();
                    item.DependsOn(openSSLCopyStandardHeaders);
                });

            source.Children.Where(item => item.InputPath.Parse().EndsWith("dbwrapper.cpp")).ToList().ForEach(item =>
                {
                    item.PrivatePatch(settings =>
                        {
                            var compiler = settings as C.ICommonCompilerSettings;
                            compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src/leveldb/helpers/memenv"));
                        });
                });
        }
    }
}
