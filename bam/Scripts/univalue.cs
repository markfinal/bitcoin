using Bam.Core;
namespace bitcoin
{
    sealed class univalue :
        C.StaticLibrary
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            this.Macros["libprefix"] = TokenizedString.CreateVerbatim("lib");

            this.CreateHeaderContainer("$(packagedir)/src/univalue/lib/*.h");
            var source = this.CreateCxxSourceContainer("$(packagedir)/src/univalue/lib/*.cpp");

            if (this.BuildEnvironment.Platform.Includes(EPlatform.Windows))
            {
                source.PrivatePatch(settings =>
                    {
                        var compiler = settings as C.ICommonCompilerSettings;
                        compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src/univalue/include"));

                        var cxxCompiler = settings as C.ICxxOnlyCompilerSettings;
                        cxxCompiler.ExceptionHandler = C.Cxx.EExceptionHandler.Asynchronous;

                        var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                        if (null != vcCompiler)
                        {
                            vcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level2;
                        }
                    });
            }
        }
    }
}
