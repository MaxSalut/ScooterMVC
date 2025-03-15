$(document).ready(function () {
    // Перевизначаємо метод валідації чисел
    $.validator.methods.number = function (value, element) {
        // Дозволяємо як кому, так і крапку як роздільник
        return this.optional(element) || !isNaN(parseFloat(value.replace(',', '.')));
    };

    // Оновлюємо правила валідації для всіх форм
    $.validator.unobtrusive.parse();
});