using System.Text.RegularExpressions;

namespace ArcWms;

public class RegexPalletCodeValidator : IPalletCodeValidator
{
    readonly string _pattern;
    readonly string _errorMessageWhenNotMatched;
    public RegexPalletCodeValidator(string pattern, string errorMessageWhenNotMatched)
    {
        _pattern = pattern;
        _errorMessageWhenNotMatched = errorMessageWhenNotMatched;
    }

    public bool IsValid(string palletCode, out string msg)
    {
        if (Regex.IsMatch(palletCode, _pattern))
        {
            msg = "OK";
            return true;
        }

        msg = _errorMessageWhenNotMatched ?? $@"与模式不匹配 {_pattern}";
        return false;
    }
}
