# csv-comparer
Compares CSV files, applying some rules.

Uses the Microsoft CSV parser to read in, line by line, two CSV files, and compare the parsed date.
It stops at the first difference, and reports the location.

A command line argument allows you to specify a number of lines to ignore before comparing, and this defaults to 1,
so a difference in the CSV header line will not be considered relevant.

A command line flag allows you to enable whitespace trimming. The Microsoft parser handles quotes automatically.
It also handles ANSI, UTF8, and Unicode with zero effort on my part, so you can compare an ANSI file against a non-ANSI
with ease.

The only allowed delimiter is comma, though it would be trivial to change this, or even make it a command line argument.

The intent of this program is to do quick comparisons of the output from R programs against the
output of queries saved from SQL Server Management Studio. It's normal to output several million lines of CSV from an
R script, and I found code comparison programs too slow, and simple file comparison useless due to subtle whitespace or
quotation differences in the files that didn't result in genuinely different data

To assist easy comparison with R output, it treats an empty string as equivalent to the text 'NULL'.

R doesn't output the word 'NULL', but SQL does; this extra functionality saves you the bother of adding
R code to convert all the empty strings into 'NULL' explicitly before saving out the CSV, and the handling of quotes
means you don't have to turn off quoting in R, which obviously has associated problems when the quotes are necessary.


```
CsvCompare usage: CsvCompare [-s <skip-line-count>] [-t] [-?|-h|--help] <left-file-name> <right-file-name>
    -s <line-count>   :  Skip the specified number of initial lines before comparison (defaults to 1)
    -t                :  trim whitespace
    -? | -h | --help  :  show this help
    
 You must specify two file names. This uses my Linuxish command line parser, and so you can write \ followed
 by a space, to escape spaces when paths with spaces in,  and despite this, backslashes will still work as
 directory separators without escaping each one. Magical!
 Alternatively, you can use forward slashes in paths, Linux style, and they'll be converted to backslashes.
 You can also quote paths normally, Microsoft style.
 ```
