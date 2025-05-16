import { createFileRoute } from '@tanstack/react-router'

export const Route = createFileRoute('/_mainLayout/profile/$profileId/')({
  component: RouteComponent,
})

function RouteComponent() {
  return <div>Hello "/_mainLayout/profile/prodileId/"!</div>
}
