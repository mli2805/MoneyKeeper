﻿Feature: MoveTransactions
	
Scenario: Сдвинуть транзакцию вверх внутри чека
	Given Существует набор данных
	And Открываю форму транзакций
	When Я двигаю не первую транзакцию чека вверх
	Then Она меняется местами с вышестоявшей
