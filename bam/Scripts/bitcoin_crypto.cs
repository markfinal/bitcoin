using Bam.Core;
namespace bitcoin
{
    sealed class bitcoin_crypto :
        C.StaticLibrary
    {
        protected override void
        Init(
            Module parent)
        {
            base.Init(parent);

            this.Macros["libprefix"] = TokenizedString.CreateVerbatim("lib");

            this.CreateHeaderContainer("$(packagedir)/src/crypto/*.h");
            var source = this.CreateCxxSourceContainer("$(packagedir)/src/crypto/hmac_sha256.cpp");
            source.AddFiles("$(packagedir)/src/crypto/hmac_sha512.cpp");
            source.AddFiles("$(packagedir)/src/crypto/ripemd160.cpp");
            source.AddFiles("$(packagedir)/src/crypto/sha1.cpp");
            source.AddFiles("$(packagedir)/src/crypto/sha256.cpp");
            source.AddFiles("$(packagedir)/src/crypto/sha512.cpp");

            source.PrivatePatch(settings =>
                {
                    var compiler = settings as C.ICommonCompilerSettings;
                    compiler.IncludePaths.AddUnique(this.CreateTokenizedString("$(packagedir)/src"));
                });
        }
    }
}
