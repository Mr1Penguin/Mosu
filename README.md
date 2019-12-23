# Mosu
Simple mock framework designed to work with UWP in release mode

This project was created as part of another bigger project.
The purpose of it is to replace Moq in UWP test projects so I could run tests in release mode where you unable to generate code in runtime.

Number of features implemented is based on needs of main project.

In plans to add mock class generation automaticaly before building test project. But I need to figure out how to analize project to search for interfaces and methods inside it ;).

Example of usage could be found in Mosu.Tests directory.
