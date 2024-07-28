#!/usr/bin/env nu
# This script generates restricted.xml
# This is a nushell script, you need to have nushell installed to run it.

def conv [path: string, headercolor: string] {
    let f = open -r $path | from csv -n -s '|'
    let cols = $f | columns
    let width = $cols | length
    print $"  <Table Columns=\"($width)\">"
    $f | first | values | each { |title|
        print $"    <ColorBox Color="($headercolor)"><Box>($title)</Box></ColorBox>"
    }
    $f | skip 1 | each { |row|
        $row | values | each { |it|
            print $"    <ColorBox><Box>($it)</Box></ColorBox>"
        }
    }
    print "  </Table>"
}

print "
<Document>
  # List of Restricted Items

  [color=lightblue][bold]OOC[/bold]: It is strongly recommended to increase the size of your guidebook window when viewing this page.[/color]

  ## Contraband
"
conv contraband.tbl "#660000"
print "
  ## Restricted Gear
"
conv restricted.tbl "#783f04"
print "
  ## Controlled Substances
"
conv substances.tbl "#4c1130"
print "</Document>"
