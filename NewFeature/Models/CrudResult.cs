using System.Collections.Generic;
using System.Linq;

namespace NewFeature.Models
{
    // One rejected field on a create/update from a department page. Field is the DTO property name
    // (camelCase, as the page sends it) so the form can put the message under the right input.
    public class FieldErrorDto
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // Outcome of a create/update on a department record. The rules behind Errors are the same ones
    // the Excel import applies to each row, so a record typed in by hand can never be accepted in a
    // shape the template upload would have rejected (or the other way round).
    public class CrudResult<T>
    {
        public T? Item { get; set; }
        public bool NotFound { get; set; }
        public List<FieldErrorDto> Errors { get; set; } = new();

        public bool Success => !NotFound && Errors.Count == 0;

        public static CrudResult<T> Ok(T item) => new() { Item = item };
        public static CrudResult<T> Missing() => new() { NotFound = true };
        public static CrudResult<T> Invalid(List<FieldErrorDto> errors) => new() { Errors = errors };

        // The response body a controller returns for a rejected create/update.
        public object ToErrorBody() => new
        {
            message = Errors.Count == 1 ? Errors[0].Message : "يرجى تصحيح الحقول المشار إليها.",
            errors = Errors
        };
    }

    public static class FieldErrors
    {
        public static void Add(this List<FieldErrorDto> list, string field, string message) =>
            list.Add(new FieldErrorDto { Field = field, Message = message });

        // Collapses a row's field errors into the single message an Excel import row reports.
        public static string Joined(this List<FieldErrorDto> list) =>
            string.Join(" ", list.Select(e => e.Message));
    }
}
