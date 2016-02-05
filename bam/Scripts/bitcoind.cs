using Bam.Core;
namespace bitcoin
{
    sealed class bitcoind :
        C.Cxx.ConsoleApplication
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            var source = this.CreateCxxSourceContainer("$(packagedir)/src/bitcoind.cpp");

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src/univalue/include"));
                });

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
                this.CompilePubliclyAndLinkAgainst<WindowsSDK.WindowsSDK>(source);

                this.PrivatePatch(settings =>
                    {
                        var linker = settings as C.ICommonLinkerSettings;
                        linker.Libraries.AddUnique("Ws2_32.lib");
                        linker.Libraries.AddUnique("User32.lib");
                        linker.Libraries.AddUnique("Shlwapi.lib");
                        linker.Libraries.AddUnique("Gdi32.lib");
                    });
            }

            this.CompileAndLinkAgainst<boost.FileSystem>(source);
            this.CompileAndLinkAgainst<boost.System>(source);
            this.CompileAndLinkAgainst<boost.Chrono>(source);
            this.CompileAndLinkAgainst<boost.Thread>(source);
            this.CompileAndLinkAgainst<boost.DateTime>(source);
            this.CompileAndLinkAgainst<boost.ProgramOptions>(source);

            this.LinkAgainst<bitcoin_crypto>();
            this.LinkAgainst<bitcoin_util>();
            this.LinkAgainst<bitcoin_common>();
            this.LinkAgainst<bitcoin_server>();
            this.LinkAgainst<univalue>();
            this.LinkAgainst<leveldb>();
            this.LinkAgainst<secp256k1>();
            
            // TODO: this shouldn't be required, because dependent modules use it, but this does seem necessary
            this.LinkAgainst<openssl.OpenSSL>();
            this.LinkAgainst<libevent.libevent>();
        }
    }
}
