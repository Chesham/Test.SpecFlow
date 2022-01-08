Feature: Mmf

Scenario: Create a persistent MMF
	Given a persistent MMF

Scenario: Write to and read from the MMF with a string
	Given a persistent MMF
	When write string "hello world" to the MMF
	Then read from the MMF should be string "hello world"

Scenario: Write to and read from the MMF with an integer
	Given a persistent MMF
	When write integer 5 to the MMF
	Then read integer from the MMF should be 5

Scenario: Shrink the MMF
	Given a persistent MMF
	When write string "hello world" to the MMF
	Then read from the MMF should be string "hello world"
	When reopen the MMF with double capacity
	Then read from the MMF should be string "hello world"

Scenario: Create a non-persisten MMF
	Given a non-persistent MMF

Scenario: Write to and read from the non-persisten MMF with a string
	Given a non-persistent MMF
	When write string "hello world" to the MMF
	Then read from the MMF should be string "hello world"

Scenario: Write to and read from the non-persisten MMF with an integer
	Given a non-persistent MMF
	When write integer 5 to the MMF
	Then read integer from the MMF should be 5

Scenario: Shrink the non-persisten MMF
	Given a non-persistent MMF with capacity 1
	When write string "hello world" to the MMF
	Then read from the MMF should be string "hello world"

Scenario: Random access the MMF smaller than the specific position
	Given a non-persistent MMF with capacity 1
	When write integer 5 to the position 64 from the begin of MMF
	Then read from the position 64 from the begin of MMF should be 5

Scenario: Random access the persistent MMF that capacity satisfied the specific position
	Given a persistent MMF with capacity 8
	When write integer 5 to the position 4 from the begin of MMF
	Then read from the position 4 from the begin of MMF should be 5

Scenario: Random access the persistent MMF smaller than the specific position
	Given a persistent MMF with capacity 1
	Then should occure an exception when create a random access view at position 64 and length 4

Scenario: Write a user-defined struct object to MMF
	Given a non-persistent MMF
	When write a user-defined object to MMF
	Then read a user-defined object should be equalled

Scenario: Cannot write reference object to MMF
	Given a non-persistent MMF
	Then should occure an exception when write an object to MMF which contain referenece type