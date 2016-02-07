using Bam.Core;
namespace bitcoin
{
    sealed class secp256k1 :
        C.StaticLibrary
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            this.Macros["libprefix"] = TokenizedString.CreateVerbatim("lib");

            this.CreateHeaderContainer("$(packagedir)/src/secp256k1/include/*.h");
            var source = this.CreateCSourceContainer("$(packagedir)/src/secp256k1/src/secp256k1.c");

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src/secp256k1"));

                    // mostly copied from basic-config.h
                    compiler.PreprocessorDefines.Add("USE_NUM_NONE");
                    compiler.PreprocessorDefines.Add("USE_FIELD_10X26");
                    compiler.PreprocessorDefines.Add("USE_FIELD_INV_BUILTIN");
                    compiler.PreprocessorDefines.Add("USE_SCALAR_8X32");
                    compiler.PreprocessorDefines.Add("USE_SCALAR_INV_BUILTIN");

                    compiler.PreprocessorDefines.Add("ENABLE_MODULE_RECOVERY");

                    var vcCompiler = settings as VisualCCommon.ICommonCompilerSettings;
                    if (null != vcCompiler)
                    {
                        vcCompiler.WarningLevel = VisualCCommon.EWarningLevel.Level2;
                    }
                });
        }
    }
}
