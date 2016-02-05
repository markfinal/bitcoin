using Bam.Core;
namespace bitcoin
{
    sealed class bitcoin_common :
        C.StaticLibrary
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            this.Macros["libprefix"] = TokenizedString.CreateVerbatim("lib");

            var source = this.CreateCxxSourceContainer("$(packagedir)/src/amount.cpp");
            source.AddFiles("$(packagedir)/src/arith_uint256.cpp");
            source.AddFiles("$(packagedir)/src/base58.cpp");
            source.AddFiles("$(packagedir)/src/chainparams.cpp");
            source.AddFiles("$(packagedir)/src/coins.cpp");
            source.AddFiles("$(packagedir)/src/compressor.cpp");
            source.AddFiles("$(packagedir)/src/consensus/merkle.cpp");
            source.AddFiles("$(packagedir)/src/core_read.cpp");
            source.AddFiles("$(packagedir)/src/core_write.cpp");
            source.AddFiles("$(packagedir)/src/hash.cpp");
            source.AddFiles("$(packagedir)/src/key.cpp");
            source.AddFiles("$(packagedir)/src/keystore.cpp");
            source.AddFiles("$(packagedir)/src/netbase.cpp");
            source.AddFiles("$(packagedir)/src/primitives/block.cpp");
            source.AddFiles("$(packagedir)/src/primitives/transaction.cpp");
            source.AddFiles("$(packagedir)/src/protocol.cpp");
            source.AddFiles("$(packagedir)/src/pubkey.cpp");
            source.AddFiles("$(packagedir)/src/scheduler.cpp");
            source.AddFiles("$(packagedir)/src/script/interpreter.cpp");
            source.AddFiles("$(packagedir)/src/script/script.cpp");
            source.AddFiles("$(packagedir)/src/script/script_error.cpp");
            source.AddFiles("$(packagedir)/src/script/sign.cpp");
            source.AddFiles("$(packagedir)/src/script/standard.cpp");

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src"));
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src/univalue/include"));
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src/secp256k1/include"));
                });

            this.CompileAgainst<boost.BoostHeaders>(source);

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
        }
    }
}
