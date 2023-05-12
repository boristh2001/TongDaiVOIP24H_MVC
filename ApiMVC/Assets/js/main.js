//function updateUrl() {
//    var param1 = document.getElementById("param1").value;

//    var url = "https://dial.voip24h.vn/dial?voip=76af0a0d5f8445fa649525123d713c6bc2b2f9b8&secret=1366b46c23edb28f61aeae42fd571e00&sip=124&phone=" + encodeURIComponent(param1);

//    window.location.href = url;

//}

var selectedNumbers = [];

function addNumber(number) {
	selectedNumbers.push(number);
	updateSelectedNumbers();
}

function removeNumber(index) {
	selectedNumbers.splice(index, 1);
	updateSelectedNumbers();
}

function updateSelectedNumbers() {
	var selectedNumbersElement = document.getElementById("selected-numbers");
	selectedNumbersElement.innerHTML = "";

	for (var i = 0; i < selectedNumbers.length; i++) {
		var number = selectedNumbers[i];
		var numberElement = document.createElement("div");
		numberElement.innerHTML = number;
		numberElement.onclick = (function (index) {
			return function () {
				removeNumber(index);
			}
		})(i);
		selectedNumbersElement.appendChild(numberElement);
	}
}

function getSelectedNumbers() {
	return selectedNumbers.join("");
}

const deleteButton = document.getElementById('delete');

deleteButton.addEventListener('click', () => {
	phoneNumber.value = phoneNumber.value.slice(0, -1);
});

function callApi() {
	var selectedNumbers = getSelectedNumbers();
	var apiUrl = "https://dial.voip24h.vn/dial?voip=76af0a0d5f8445fa649525123d713c6bc2b2f9b8&secret=1366b46c23edb28f61aeae42fd571e00&sip=124&phone=" + selectedNumbers;
	// gọi API bên thứ 3 với apiUrl
	window.location.href = apiUrl;
	window.history.back;
}
