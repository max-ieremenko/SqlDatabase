[CmdletBinding(SupportsShouldProcess=$true)]
param (
    $Command,
    $Variables
)

$Command.CommandText = "INSERT INTO module_a_person(name) VALUES ('John'), ('Maria')"
$Command.ExecuteNonQuery()

$Command.CommandText = "INSERT INTO module_b_person_address(person_id, city) SELECT person.id, 'London' FROM person person WHERE person.name = 'John'"
$Command.ExecuteNonQuery()
