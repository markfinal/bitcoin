using Bam.Core;
namespace bitcoin
{
    sealed class leveldb :
        C.StaticLibrary
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            this.Macros["libprefix"] = TokenizedString.CreateVerbatim("lib");

            this.CreateHeaderContainer("$(packagedir)/src/leveldb/db/*.h");
            var source = this.CreateCxxSourceContainer("$(packagedir)/src/leveldb/db/*.cc",
                filter: new System.Text.RegularExpressions.Regex(@"^((?!.*_test)(?!.*_bench)(?!.*_main).*)$"));
            source.AddFiles("$(packagedir)/src/leveldb/util/*.cc",
                filter: new System.Text.RegularExpressions.Regex(@"^((?!.*env_)(?!.*_test).*)$"));
            source.AddFiles("$(packagedir)/src/leveldb/helpers/memenv/memenv.cc");
            source.AddFiles("$(packagedir)/src/leveldb/table/*.cc",
                filter: new System.Text.RegularExpressions.Regex(@"^((?!.*_test).*)$"));

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src/leveldb"));
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src/leveldb/include"));
                });

            if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows))
            {
                source.AddFiles("$(packagedir)/src/leveldb/util/env_win.cc");
                source.AddFiles("$(packagedir)/src/leveldb/port/port_win.cc");
                source.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.PreprocessorDefines.Add("LEVELDB_PLATFORM_WINDOWS");

                        var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                        cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;
                    });

                this.CompileAgainst<WindowsSDK.WindowsSDK>(source);
            }
        }
    }
}
