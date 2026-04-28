using Microsoft.AspNetCore.Components;

namespace CaesarCalendar.Web.Components
{

    public partial class DropDown<TItem>
    {
        [Parameter]
        public RenderFragment? Label { get; set; }
        [Parameter]
        public RenderFragment? ChildContent { get; set; }
        [Parameter]
        public EventCallback<TItem?> OnSelected { get; set; }
        private bool Show { get; set; } = false;
        private void OnMouseDown()
        {
            Show = !Show;
        }

        private void OnFocusOut()
        {
            Show = false;
            StateHasChanged();
        }

        public async Task HandleSelect(TItem? item)
        {
            Show = false;
            await OnSelected.InvokeAsync(item);
        }
    }

}
