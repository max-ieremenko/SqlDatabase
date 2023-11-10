[CmdletBinding(SupportsShouldProcess=$true)]
param (
    $Command,
    $Variables
)

$Command.CommandText = "INSERT INTO moduleA.Person(Name) VALUES ('John'), ('Maria')"
$Command.ExecuteNonQuery()

$Command.CommandText = "INSERT INTO moduleB.PersonAddress(PersonId, City) SELECT Person.Id, 'London' FROM demo.Person Person WHERE Person.Name = 'John'"
$Command.ExecuteNonQuery()
