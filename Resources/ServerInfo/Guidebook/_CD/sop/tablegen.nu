#!/usr/bin/env nu
# This script generates Guidebook Markup tables from TSV files.
# This is a nushell script, you need to have nushell installed to run it.
# It takes a path to the TSV file to convert as it's first argument

def main [path: string] {
    let f = open -r $path | from tsv -n
    let cols = $f | columns
    let width = $cols | length
    print $"  <Table Columns=\"($width)\">"
    $f | first | values | each { |title|
        print $"    <ColorBox Color="#4A148C"><Box>($title)</Box></ColorBox>"
    }
    $f | skip 1 | each { |row|
        $row | values | each { |it|
            print $"    <ColorBox><Box>($it)</Box></ColorBox>"
        }
    }
    print "  </Table>"
}
