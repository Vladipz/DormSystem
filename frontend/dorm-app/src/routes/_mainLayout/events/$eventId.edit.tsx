import { createFileRoute, useParams } from '@tanstack/react-router'

export const Route = createFileRoute('/_mainLayout/events/$eventId/edit')({
  component: RouteComponent,
})

function RouteComponent() {
  const { eventId } = useParams({ from: '/_mainLayout/events/$eventId/edit' })
  
  return (
    <div className='h-[1000px]'>
      Event ID: {eventId} edit page
    </div>
  )
}
