open System
open System.Text.RegularExpressions

let strings = "03.01.;04.05;25.6."

let getBillingDates (start : DateTime) (``end`` : DateTime) ``string`` =
    let regex = Regex "^(?<day>\d{1,2})\.(?<month>\\d{1,2}).(\\d{4})?$"
    let ``match`` = regex.Match(``string``);
    let day = int (``match``.Groups["day"].Captures[0].Value);
    let month = int (``match``.Groups["month"].Captures[0].Value);


