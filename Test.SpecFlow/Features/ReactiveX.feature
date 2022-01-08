Feature: Rx Testing

Scenario: Publish and Connect
	Given an observable will emmit 5 publications
	When subscribe the observable
	And subscribe the observable again
	Then the last two observers should have same publications of 5